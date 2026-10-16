using System.Collections.Generic;
using UnityEngine;

// The board as plain data: size, obstacles and which cells a unit stands on.
// why: no MonoBehaviour here, so the rules (pathfinding, the enemy's turn, the invariants) can be tested
// in a plain C# test, without opening Unity. GridManager owns one of these and forwards to it.
public class GridState
{
    public int SizeX { get; }
    public int SizeZ { get; }

    private readonly bool[] obstacles;
    private readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    // obstacles is row-major, index = z * sizeX + x (the layout ObstacleData and ObstacleEditor use).
    // A missing or short array means "no obstacle" for the cells it doesn't cover.
    public GridState(int sizeX, int sizeZ, bool[] obstacles)
    {
        SizeX = sizeX;
        SizeZ = sizeZ;
        this.obstacles = obstacles ?? new bool[0];
    }

    public bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < SizeX && z >= 0 && z < SizeZ;
    }

    public bool IsObstacle(int x, int z)
    {
        if (!IsInsideGrid(x, z)) return false;
        int index = z * SizeX + x;
        return index < obstacles.Length && obstacles[index];
    }

    public bool IsOccupied(int x, int z)
    {
        return occupied.Contains(new Vector2Int(x, z));
    }

    // inside the board, not an obstacle, nobody standing there
    public bool IsWalkable(int x, int z)
    {
        return IsInsideGrid(x, z) && !IsObstacle(x, z) && !IsOccupied(x, z);
    }

    public void SetOccupied(int x, int z) { occupied.Add(new Vector2Int(x, z)); }
    public void SetFree(int x, int z) { occupied.Remove(new Vector2Int(x, z)); }

    // A copy, so a check can't change the board by accident.
    public IReadOnlyCollection<Vector2Int> OccupiedCells()
    {
        return new List<Vector2Int>(occupied);
    }
}
