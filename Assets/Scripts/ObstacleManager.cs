using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    // reference to our obstacle data
    public ObstacleData obstacleData;
    // your custom prefab for obstacles
    public GameObject obstaclePrefab;
    private int y = 1; // spawn height of the obstacles
    
    void Start()
    { 
        CreateObstacles(); // call the function to create obstacles
    }
    
    void CreateObstacles()
    {
        // for loop to go through the grid
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                int arrayPosition = z * 10 + x; // calculate array position cuz we are using a 1D array to store obstacles
                bool hasObstacle = obstacleData.gridObstacles[arrayPosition]; // checks from obstacleData(scriptable object) if there is an obstacle at that position
                if (hasObstacle == true)// if true spawn the obstacle
                {
                    Vector3 spawnPosition = new Vector3(x, y, z); 
                    GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
                    newObstacle.name = "Obstacle: " + x + ", " + z;
                }
            }
        }
    }
}