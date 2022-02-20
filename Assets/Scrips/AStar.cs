using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // Import JSON.NET from Unity Asset store (not needed in 2022 version)

public class AStar : MonoBehaviour {

    private class Node
    {

        public Node parent;
        public Vector2Int position;
        public float g;
        public float h;
        public float f;

        public Node(Node parent, Vector2Int position) 
        {
            this.parent = parent;
            this.position = position;
            this.g = 0f;
            this.h = 0f;
            this.f = 0f;
        }

        public override bool Equals(object other)
        {
            return this.position.Equals((other as Node).position);
        }

        public override int GetHashCode() {
            return this.position.GetHashCode();
        }

    }

    public static float[,] getGrid(float[,] grid_original, int scale, int pad, TerrainManager terrain_manager)
    {
        float[,] grid_upsampled = new float[grid_original.GetLength(0) * scale, grid_original.GetLength(1) * scale];
        float x_h = (terrain_manager.myInfo.x_high - terrain_manager.myInfo.x_low)/terrain_manager.myInfo.x_N;
        float z_h = (terrain_manager.myInfo.z_high - terrain_manager.myInfo.z_low)/terrain_manager.myInfo.z_N;
        
        for (int i = 0; i < grid_original.GetLength(0); i++)
        {   
            for (int j = 0; j < grid_original.GetLength(1); j++)
            {
                if (grid_original[i,j] == 1.0f)
                {
                    int x1 = i * scale;
                    int x2 = i * scale + scale;
                    int z1 = j * scale;
                    int z2 = j * scale + scale;

                    for (int ix = x1; ix < x2; ix++)
                    {
                        for (int iz = z1; iz < z2; iz++)
                        {
                            grid_upsampled[ix, iz] = 1.0f;
                        }
                    }
                }
            }
        }

        float[,] grid_inflated = new float[grid_upsampled.GetLength(0), grid_upsampled.GetLength(1)];
        
        for (int i = pad; i < grid_upsampled.GetLength(0)-pad; i++)
        {
            for (int j = pad; j < grid_upsampled.GetLength(1)-pad; j++)
            {
                if (i == j && i != 0)
                {
                    continue;
                }
                if (grid_upsampled[i, j] == 1.0f)
                {
                    for (int ii = -pad; ii <= pad; ii++)
                    {
                        for (int jj = -pad; jj <= pad; jj++)
                        {
                            grid_inflated[i+ii, j+jj] = 1.0f;
                        }
                    }
                }
            }
        }
        
        return grid_inflated;
    }

    public static Vector2Int getGridPosition(Vector3 pos, int scale, TerrainManager tm)
    {
        float[,] grid = tm.myInfo.traversability;

        int ixp = (int) Mathf.Floor((grid.GetLength(0) * scale) * (pos.x-tm.myInfo.x_low)/(tm.myInfo.x_high-tm.myInfo.x_low));
        int izp = (int) Mathf.Floor((grid.GetLength(1) * scale) * (pos.z-tm.myInfo.z_low)/(tm.myInfo.z_high-tm.myInfo.z_low));
        Vector2Int position = new Vector2Int(ixp, izp);

        return position;
    }

    public static Vector3 getUnityPosition(Vector2Int pos, int scale, TerrainManager tm)
    {
        float[,] grid = tm.myInfo.traversability;

        float xstep = (tm.myInfo.x_high - tm.myInfo.x_low) / (grid.GetLength(0) * scale);
        float zstep = (tm.myInfo.z_high - tm.myInfo.z_low) / (grid.GetLength(1) * scale);
        float uxp = tm.myInfo.x_low + xstep / 2 + xstep * pos.x;
        float uzp = tm.myInfo.z_low + zstep / 2 + zstep * pos.y;
        Vector3 position = new Vector3(uxp, 0f, uzp);

        return position;
    }


    public static List<Vector2Int> getPath(float[,] grid, Vector2Int start, Vector2Int goal)
    {
        Node startNode = new Node(null, start);
        Node goalNode = new Node(null, goal);

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(startNode);

        while (open.Count > 0)
        {
            Node currentNode = open[0];
            int currentIndex = 0;
            for (int i = 0; i < open.Count; i++)
            {
                if (open[i].f < currentNode.f) 
                {
                    currentNode = open[i];
                    currentIndex = i;
                }
            }

            open.RemoveAt(currentIndex);
            closed.Add(currentNode);

            if (currentNode.Equals(goalNode)) 
            {
                List<Vector2Int> path = new List<Vector2Int>();
                while (currentNode != null) {
                    path.Add(currentNode.position);
                    currentNode = currentNode.parent;
                }
                path.Reverse();
                return path;
            }

            List<Node> children = new List<Node>();
            for (int x = -1; x <= 1; x++) 
            {
                for (int z = -1; z <= 1; z++) 
                {
                    if (x == 0 && z == 0) 
                    {continue;}

                    Vector2Int nodePosition = currentNode.position + new Vector2Int(x, z);
                    if (nodePosition[0] >= grid.GetLength(0) || nodePosition[0] < 0 || nodePosition[1] >= grid.GetLength(1) || nodePosition[1] < 0)
                    {continue;}

                    if (grid[nodePosition[0], nodePosition[1]] != 0)
                    {continue;}

                    Node newNode = new Node(currentNode, nodePosition);
                    children.Add(newNode);
                }
            }

            foreach (Node child in children)
            {
                if (closed.Contains(child))
                {continue;}

                child.g = currentNode.g + 1;
                child.h = Vector2Int.Distance(child.position, goalNode.position);
                child.f = child.g + child.h;

                if (open.Contains(child)) 
                {
                    int childIndex = open.IndexOf(child);
                    if (child.g > open[childIndex].g) 
                    {
                        continue;
                    } else 
                    {
                        open.RemoveAt(childIndex);
                    }
                }

                open.Add(child);
            }
        }

        return new List<Vector2Int>();
    }

}