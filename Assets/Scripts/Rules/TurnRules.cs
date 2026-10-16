using System.Collections.Generic;
using UnityEngine;

// The rules of a turn, without any Unity objects: who may go where, and what the enemy does.
// EnemyAI and PlayerUnit call these, and so do the tests, so the game and the tests can't drift apart.
public static class TurnRules
{
    // 4 directions, no diagonals (same order the pathfinder uses)
    public static readonly Vector2Int[] Around =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    public static int Distance(Vector2Int a, Vector2Int b)
    {
        return System.Math.Abs(a.x - b.x) + System.Math.Abs(a.y - b.y);
    }

    public static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Distance(a, b) == 1;
    }

    // Can a unit standing on `from` move at all? If not, it is trapped.
    public static bool HasAnyMove(GridState grid, Vector2Int from)
    {
        foreach (Vector2Int dir in Around)
        {
            Vector2Int next = from + dir;
            if (grid.IsWalkable(next.x, next.y)) return true;
        }
        return false;
    }

    // Every cell a unit on `from` can reach this turn (any distance: the player has no move limit).
    // why: the QA bot picks its move from this list, the same way a player picks a tile to click.
    public static List<Vector2Int> LegalDestinations(GridState grid, Vector2Int from)
    {
        var result = new List<Vector2Int>();
        for (int z = 0; z < grid.SizeZ; z++)
        {
            for (int x = 0; x < grid.SizeX; x++)
            {
                var cell = new Vector2Int(x, z);
                if (cell == from || !grid.IsWalkable(x, z)) continue;
                if (Pathfinder.FindPath(from, cell, grid).Count > 0) result.Add(cell);
            }
        }
        return result;
    }

    // The enemy's turn: walk towards the closest free tile next to the player, at most `budget` steps,
    // and stop as soon as it stands next to the player. Returns the cells it walks, in order.
    public static List<Vector2Int> PlanEnemyTurn(GridState grid, Vector2Int enemy, Vector2Int player, int budget)
    {
        var steps = new List<Vector2Int>();
        if (IsAdjacent(enemy, player)) return steps;

        List<Vector2Int> path = PathToClosestTileNextTo(grid, enemy, player);
        int count = System.Math.Min(budget, path.Count);
        for (int i = 0; i < count; i++)
        {
            steps.Add(path[i]);
            if (IsAdjacent(path[i], player)) break;
        }
        return steps;
    }

    // Shortest path to any free tile next to `target`; ties go to the first tile in `Around` order.
    private static List<Vector2Int> PathToClosestTileNextTo(GridState grid, Vector2Int from, Vector2Int target)
    {
        List<Vector2Int> best = new List<Vector2Int>();
        foreach (Vector2Int offset in Around)
        {
            Vector2Int cell = target + offset;
            if (!grid.IsWalkable(cell.x, cell.y)) continue;

            List<Vector2Int> path = Pathfinder.FindPath(from, cell, grid);
            if (path.Count == 0) continue;
            if (best.Count == 0 || path.Count < best.Count) best = path;
        }
        return best;
    }
}
