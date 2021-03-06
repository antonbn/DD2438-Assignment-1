using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // Import JSON.NET from Unity Asset store (not needed in 2022 version)


public class TerrainManager : MonoBehaviour {


    //public TestScriptNoObject testNoObject = new TestScriptNoObject();

    public string terrain_filename = "Text/terrain";
    public TerrainInfo myInfo;
    public List<Vector3> my_path;

    public GameObject flag;

    // Use this for initialization
    void Awake()
    {

        var jsonTextFile = Resources.Load<TextAsset>(terrain_filename);

        // set to false for hard coding new terrain using TerrainInfo2 below
        bool loadFromFile = true;

        if (loadFromFile)
        {
            myInfo = TerrainInfo.CreateFromJSON(jsonTextFile.text);
        }
        else
        {
            myInfo.TerrainInfo2();
            //myInfo.file_name = "test88";
            string myString = myInfo.SaveToString();
            myInfo.WriteDataToFile(myString);
        }

        myInfo.CreateCubes();

        Instantiate(flag, myInfo.start_pos, Quaternion.identity);
        Instantiate(flag, myInfo.goal_pos, Quaternion.identity);

        my_path = getUnityPath();

    }

    private List<Vector3> getUnityPath()
    {
        float[,] grid = myInfo.traversability;

        int scale = 14;//(int) 200 / Mathf.Max(grid.GetLength(0), grid.GetLength(1));
        float[,] my_grid = AStar.getGrid(grid, scale, 2, this);

        Vector2Int start_pos_inflated = AStar.getGridPosition(myInfo.start_pos, scale, this);
        Vector2Int goal_pos_inflated = AStar.getGridPosition(myInfo.goal_pos, scale, this);

        List<Vector2Int> my_path = AStar.getPath(my_grid, start_pos_inflated, goal_pos_inflated);

        List<Vector3> unity_path = new List<Vector3>();
        Vector3 old_grid_center = myInfo.start_pos;
        foreach (Vector2Int wp in my_path)
        {
            Vector3 grid_center = AStar.getUnityPosition(wp, scale, this);
            unity_path.Add(grid_center);

            Debug.DrawLine(old_grid_center, grid_center, Color.red, Mathf.Infinity);
            old_grid_center = grid_center;
        }

        return unity_path;
    }

}



[System.Serializable]
public class TerrainInfo
{
    public string file_name;
    public float x_low;
    public float x_high;
    public int x_N;
    public float z_low;
    public float z_high;
    public int z_N;
    public float[,] traversability;

    public Vector3 start_pos;
    public Vector3 goal_pos;

    
    public void TerrainInfo2()
    {
        file_name = "my_terrain";
        x_low = 50f;
        x_high = 250f;
        x_N = 45;
        z_low = 50f;
        z_high = 250f;
        z_N = 7;

        start_pos = new Vector3(100f, 1f, 100f);
        goal_pos = new Vector3(110f, 1f, 110f);


        Debug.Log("Using hard coded info...");
        //traversability = new float[,] { { 1.1f, 2f }, { 3.3f, 4.4f } };
        traversability = new float[x_N, z_N]; // hardcoded now, needs to change
        for(int i = 0; i < x_N; i++)
        {
            for (int j = 0; j < z_N; j++)
            {
                if ((i == 0 || i == x_N -1) || (j == 0 || j == z_N - 1))
                {
                    traversability[i, j] = 1.0f;
                }
                else
                {
                    traversability[i, j] = 0.0f;
                }
            }
        }
    }

    public int get_i_index(float x)
    {
        int index = (int) Mathf.Floor(x_N * (x - x_low) / (x_high - x_low));
        if (index < 0)
        {
            index = 0;
        }else if (index > x_N - 1)
        {
            index = x_N - 1;
        }
        return index;

    }
    public int get_j_index(float z) // get index of given coordinate
    {
        int index = (int)Mathf.Floor(z_N * (z - z_low) / (z_high - z_low));
        if (index < 0)
        {
            index = 0;
        }
        else if (index > z_N - 1)
        {
            index = z_N - 1;
        }
        return index;
    }

    public float get_x_pos(int i)
    {
        float step = (x_high - x_low) / x_N;
        return x_low + step / 2 + step * i;
    }

    public float get_z_pos(int j) // get position of given index
    {
        float step = (z_high - z_low) / z_N;
        return z_low + step / 2 + step * j;
    }

    public void CreateCubes()
    {
        float x_step = (x_high - x_low) / x_N;
        float z_step = (z_high - z_low) / z_N;
        for (int i = 0; i < x_N; i++)
        {
            for (int j = 0; j < z_N; j++)
            {
                if (traversability[i, j] > 0.5f)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(get_x_pos(i), 0.0f, get_z_pos(j));
                    cube.transform.localScale = new Vector3(x_step, 15.0f, z_step);
                    cube.tag = "wall";
                }

            }
        }
    }



    public static TerrainInfo CreateFromJSON(string jsonString)
    {
        //Debug.Log("Reading json");
        return JsonConvert.DeserializeObject<TerrainInfo>(jsonString);
        //return JsonUtility.FromJson<TerrainInfo>(jsonString);
    }

    public string SaveToString()
    {
        JsonSerializerSettings JSS = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };

        return JsonConvert.SerializeObject(this, Formatting.None, JSS);

        //return JsonConvert.SerializeObject(this); // throws ref loop error
        //return JsonUtility.ToJson(this); // cannot handle multi-dim arrays
    }

    public void WriteDataToFile(string jsonString)
    {
        string path = Application.dataPath + "/Resources/Text/" + file_name + ".json";
        Debug.Log("AssetPath:" + path);
        System.IO.File.WriteAllText(path, jsonString);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }


}