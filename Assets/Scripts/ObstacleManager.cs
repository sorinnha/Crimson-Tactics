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
        // why: read the board size from GridManager instead of assuming 10x10, so obstacles and
        // pathfinding always agree on where the blocked cells are (they share the z * sizeX + x layout)
        GridManager grid = FindFirstObjectByType<GridManager>();
        int sizeX = grid != null ? grid.gridSizeX : 10;
        int sizeZ = grid != null ? grid.gridSizeZ : 10;

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                int arrayPosition = z * sizeX + x; // 1D array: row by row
                if (arrayPosition >= obstacleData.gridObstacles.Length) continue; // data smaller than the grid

                if (obstacleData.gridObstacles[arrayPosition]) // checks from obstacleData (scriptable object)
                {
                    Vector3 spawnPosition = new Vector3(x, y, z);
                    GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
                    newObstacle.name = "Obstacle: " + x + ", " + z;
                }
            }
        }
    }
}
