using System.Collections.Generic;
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

    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;

        if (obstacleData == null)
        {
            ObstacleManager manager = FindFirstObjectByType<ObstacleManager>();
            if (manager != null) obstacleData = manager.obstacleData;
        }
    }

    public bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ;
    }

    // same layout used in ObstacleManager and ObstacleEditor
    private int ToIndex(int x, int z)
    {
        return z * gridSizeX + x;
    }

    public bool IsObstacle(int x, int z)
    {
        if (obstacleData == null) return false;
        int index = ToIndex(x, z);
        if (index < 0 || index >= obstacleData.gridObstacles.Length) return false;
        return obstacleData.gridObstacles[index];
    }

    public bool IsWalkable(int x, int z)
    {
        if (!IsInsideGrid(x, z)) return false;
        if (IsObstacle(x, z)) return false;
        if (occupiedCells.Contains(new Vector2Int(x, z))) return false;
        return true;
    }

    public bool IsOccupied(int x, int z)
    {
        return occupiedCells.Contains(new Vector2Int(x, z));
    }

    public void SetOccupied(int x, int z) { occupiedCells.Add(new Vector2Int(x, z)); }
    public void SetFree(int x, int z) { occupiedCells.Remove(new Vector2Int(x, z)); }

    public Vector3 CellToWorld(int x, int z, float y)
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.z));
    }
}
