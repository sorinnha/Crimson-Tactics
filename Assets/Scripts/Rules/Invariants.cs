using System.Collections.Generic;
using UnityEngine;

// Things that must always be true between turns. The QA probe checks them after every turn in the real
// game, and the tests check them over thousands of simulated games. Each problem comes back as a short
// sentence a tester could paste into a bug report.
public static class Invariants
{
    // The board while nobody is moving: each unit on its own free-standing cell, and the grid's
    // "occupied" set holds exactly the units' cells (a leftover cell would block paths for no reason).
    public static List<string> CheckBoard(GridState grid, IReadOnlyDictionary<string, Vector2Int> units)
    {
        var problems = new List<string>();
        var seen = new Dictionary<Vector2Int, string>();

        foreach (KeyValuePair<string, Vector2Int> unit in units)
        {
            Vector2Int c = unit.Value;
            if (!grid.IsInsideGrid(c.x, c.y)) problems.Add($"{unit.Key} is outside the grid at {c.x},{c.y}");
            else if (grid.IsObstacle(c.x, c.y)) problems.Add($"{unit.Key} stands on an obstacle at {c.x},{c.y}");

            if (seen.TryGetValue(c, out string other)) problems.Add($"{unit.Key} and {other} share cell {c.x},{c.y}");
            else seen[c] = unit.Key;

            if (!grid.IsOccupied(c.x, c.y)) problems.Add($"{unit.Key}'s cell {c.x},{c.y} is not marked occupied");
        }

        foreach (Vector2Int c in grid.OccupiedCells())
        {
            if (!seen.ContainsKey(c)) problems.Add($"cell {c.x},{c.y} is marked occupied but nobody stands there");
        }
        return problems;
    }

    // After the enemy's turn: it may not walk further than the player did, and the game is over exactly
    // when it ended next to the player.
    public static List<string> CheckEnemyTurn(int playerSteps, int enemySteps, bool adjacentAfter, bool gameOver)
    {
        var problems = new List<string>();
        if (enemySteps > playerSteps)
            problems.Add($"enemy walked {enemySteps} steps after the player walked {playerSteps}");
        if (adjacentAfter && !gameOver)
            problems.Add("enemy ended next to the player but the game is not over");
        if (!adjacentAfter && gameOver)
            problems.Add("game over although the enemy is not next to the player");
        return problems;
    }
}
