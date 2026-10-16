using UnityEngine;

// holds the grid state and answers whether a cell can be walked on
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Size (keep this the same as the TileGenerator)")]
    public int gridSizeX = 10;
    public int gridSizeZ = 10;

    [Header("Obstacle Source")]
    [Tooltip("Leave empty to automatically grab it from the ObstacleManager in the scene.")]
    public ObstacleData obstacleData;

    // The board itself lives in a plain C# object so the rules can be tested without Unity.
    public GridState State { get; private set; }

    void Awake()
    {
        Instance = this;

        if (obstacleData == null)
        {
            ObstacleManager manager = FindFirstObjectByType<ObstacleManager>();
            if (manager != null) obstacleData = manager.obstacleData;
        }

        State = new GridState(gridSizeX, gridSizeZ, obstacleData != null ? obstacleData.gridObstacles : null);
    }

    public bool IsInsideGrid(int x, int z) { return State.IsInsideGrid(x, z); }
    public bool IsObstacle(int x, int z) { return State.IsObstacle(x, z); }
    public bool IsWalkable(int x, int z) { return State.IsWalkable(x, z); }
    public bool IsOccupied(int x, int z) { return State.IsOccupied(x, z); }
    public void SetOccupied(int x, int z) { State.SetOccupied(x, z); }
    public void SetFree(int x, int z) { State.SetFree(x, z); }

    public Vector3 CellToWorld(int x, int z, float y)
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.z));
    }
}
