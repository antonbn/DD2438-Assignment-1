using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityStandardAssets.Vehicles.Car
{

    [RequireComponent(typeof(CarController))]
    public class CarAI : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        public GameObject sky_car;
        TerrainManager terrain_manager;

        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();

            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            // Plan your path here
            // Replace the code below that makes a random path
            // ...

            /*float[,] grid = terrain_manager.myInfo.traversability;

            Vector3 start_pos = terrain_manager.myInfo.start_pos;
            int startI = terrain_manager.myInfo.get_i_index(start_pos.x);
            int startJ = terrain_manager.myInfo.get_j_index(start_pos.z);
            Vector2Int start_pos_grid = new Vector2Int(startI, startJ);

            Vector3 goal_pos = terrain_manager.myInfo.goal_pos;
            int goalI = terrain_manager.myInfo.get_i_index(goal_pos.x);
            int goalJ = terrain_manager.myInfo.get_j_index(goal_pos.z);
            Vector2Int goal_pos_grid = new Vector2Int(goalI, goalJ);

            List<Vector2Int> my_path = AStar.getPath(grid, start_pos_grid, goal_pos_grid);

            // Plot your path to see if it makes sense
            // Note that path can only be seen in "Scene" window, not "Game" window
            float old_grid_center_x = terrain_manager.myInfo.get_x_pos(my_path[0][0]);
            float old_grid_center_z = terrain_manager.myInfo.get_z_pos(my_path[0][1]);
            foreach (Vector2Int wp in my_path)
            {
                float grid_center_x = terrain_manager.myInfo.get_x_pos(wp[0]);
                float grid_center_z = terrain_manager.myInfo.get_z_pos(wp[1]);

                Debug.DrawLine(new Vector3(old_grid_center_x, 1, old_grid_center_z), new Vector3(grid_center_x, 1, grid_center_z), Color.red, 100f);
                old_grid_center_x = grid_center_x;
                old_grid_center_z = grid_center_z;
            }*/

        }


        private void FixedUpdate()
        {
            // Execute your path here
            // ...

            // this is how you access information about the terrain from the map
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z));

            // this is how you access information about the terrain from a simulated laser range finder
            RaycastHit hit;
            float maxRange = 50f;
            if (Physics.Raycast(transform.position + transform.up, transform.TransformDirection(Vector3.forward), out hit, maxRange))
            {
                Vector3 closestObstacleInFront = transform.TransformDirection(Vector3.forward) * hit.distance;
                Debug.DrawRay(transform.position, closestObstacleInFront, Color.yellow);
                //Debug.Log("Did Hit");
            }


            // this is how you control the car
            //m_Car.Move(1f, 1f, 1f, 0f);

        }
    }
}
