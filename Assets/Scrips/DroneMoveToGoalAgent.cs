using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{

    [RequireComponent(typeof(DroneController))]
    public class DroneMoveToGoalAgent : Agent
    {

        private DroneController m_Drone; // the car controller we want to use
        Rigidbody m_AgentRb;
        TerrainManager terrain_manager;
        private List<Vector3> my_path;
        private bool collided;
        float start_time;
        
        public GameObject terrain_manager_game_object;
    

        private void Start()
        {
            m_Drone = GetComponent<DroneController>();
            m_AgentRb = GetComponent<Rigidbody>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        }

        

        public override void OnEpisodeBegin()
        {
            transform.position = terrain_manager.myInfo.start_pos + Vector3.up * 3;
            transform.rotation = Quaternion.identity;
            m_AgentRb.velocity = Vector3.zero;
            m_AgentRb.angularVelocity = Vector3.zero;
            m_Drone.Initialize();
            my_path = new List<Vector3>(terrain_manager.my_path);
            collided = false;
            start_time = Time.time;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(new Vector2(m_Drone.acceleration.x, m_Drone.acceleration.z) / m_Drone.max_acceleration);
            sensor.AddObservation(m_Drone.max_acceleration / 15f);
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
                float x = Mathf.Sin(angle);
                float z = Mathf.Cos(angle);
                angle += 2 * Mathf.PI / 12;
            
                RaycastHit hit;
                float maxRange = 10f;
                if (Physics.Raycast(transform.position, new Vector3(x, 0, z).normalized, out hit, maxRange, 1)) 
                {
                    Debug.DrawRay(transform.position, new Vector3(x, 0, z).normalized * hit.distance, Color.red);
                    sensor.AddObservation(hit.distance / maxRange);
                } else 
                {
                    sensor.AddObservation(1f);
                }
            }

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            m_Drone.Move(actions.ContinuousActions[0], actions.ContinuousActions[1]);

            AddReward(-1f / MaxStep);

            

            if ((transform.position - terrain_manager.myInfo.goal_pos).magnitude < 10f)
            {
                float completion_time = Time.time - start_time;
                Debug.Log("Completed in " + completion_time + "s");
                
                AddReward(2f);
                EndEpisode();
            }

            int i = my_path.Count - 1;
            while (i != -1 && Vector3.Distance(transform.position,  my_path[i]) >= 5f)
            {
                i--;
            }
            my_path.RemoveRange(0, i + 1);
            AddReward((i + 1) * 0.1f);

            if (collided)
            {
                collided = false;
                AddReward(-0.1f);
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