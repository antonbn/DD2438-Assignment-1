using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class MoveToGoalAgent : Agent
    {
        private CarController m_Car; // the car controller we want to use
        private bool collided;
        private List<Vector3> my_path;
        private float start_time;
        private float completion_time;
        
        public GameObject terrain_manager_game_object;
    
        Rigidbody m_AgentRb;
        TerrainManager terrain_manager;

        private void Start()
        {
            m_AgentRb = GetComponent<Rigidbody>();
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        }

        public override void OnEpisodeBegin()
        {
            transform.position = terrain_manager.myInfo.start_pos + Vector3.up * 0.06f;
            transform.rotation = Quaternion.identity;
            m_AgentRb.velocity = Vector3.zero;
            m_AgentRb.angularVelocity = Vector3.zero;
            m_Car.Initialize();
            my_path = new List<Vector3>(terrain_manager.my_path);
            collided = false;
            start_time = Time.time;
            
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 velocity_relative = transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(new Vector2(velocity_relative.x, velocity_relative.z) * 2.23693629f / m_Car.MaxSpeed);
            //sensor.AddObservation(new Vector2(transform.forward.x, transform.forward.y).normalized);
            sensor.AddObservation(new Vector2(transform.forward.x, transform.forward.z));
            foreach (int checkpoint in new int[]{0, 3, 6, 9})
            {
                int index = (int) Mathf.Min(checkpoint, my_path.Count - 1);
                Vector3 checkpoint_direction;
                if (index != -1)
                {
                    checkpoint_direction = transform.InverseTransformDirection(my_path[index] - transform.position);
                } else
                {
                    checkpoint_direction = transform.InverseTransformDirection(terrain_manager.myInfo.goal_pos - transform.position);
                }
                sensor.AddObservation(new Vector2(checkpoint_direction.x, checkpoint_direction.z) / 200f);
            }

            float angle = 0;
            for (int i = 0; i < 12; i++) {
                angle += 360 / 12;
            
                Vector3 direction = (Quaternion.Euler(0, angle, 0) * transform.forward).normalized;
                RaycastHit hit;
                float maxRange = 10f;
                if (Physics.Raycast(transform.position, direction, out hit, maxRange, 1)) 
                {
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
                    sensor.AddObservation(hit.distance / maxRange);
                } else 
                {
                    sensor.AddObservation(1f);
                }
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            m_Car.Move(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[1], 0f);

            AddReward(-1f / MaxStep);

            if ((transform.position - terrain_manager.myInfo.goal_pos).magnitude < 10f)
            {
                float completion_time = Time.time - start_time;
                Debug.Log("Completed in " + completion_time + "s");
                
                AddReward(2f);
                EndEpisode();
            }

            int i = my_path.Count - 1;
            while (i != -1 && Vector3.Distance(transform.position,  my_path[i]) >= 10f)
            {
                i--;
            }
            my_path.RemoveRange(0, i + 1);
            AddReward((i + 1) * 10f / terrain_manager.my_path.Count);

            if (collided)
            {
                AddReward(-2.2f);
                collided = false;
            }

        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = CrossPlatformInputManager.GetAxis("Horizontal");
            continuousActions[1] = CrossPlatformInputManager.GetAxis("Vertical");
        }

        private void OnCollisionEnter(Collision other)
        {
            collided = true;
        }
    }
}