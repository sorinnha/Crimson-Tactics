using System.Collections.Generic;
using UnityEngine;

// Helpers for the rules tests: seeded random boards, a breadth-first search to compare A* against, and a
// whole match played by the rules alone.
public static class TestBoards
{
    // A sizeX x sizeZ board where each cell is an obstacle with the given chance. Same seed, same board.
    public static GridState Random(System.Random rng, int sizeX, int sizeZ, double obstacleChance)
    {
        var obstacles = new bool[sizeX * sizeZ];
        for (int i = 0; i < obstacles.Length; i++) obstacles[i] = rng.NextDouble() < obstacleChance;
        return new GridState(sizeX, sizeZ, obstacles);
    }

    // Parses rows like "..#." (top row first, like the Obstacle Editor draws it): '#' obstacle,
    // 'P' and 'E' mark cells returned in `marks` (and left walkable).
    public static GridState Parse(Dictionary<char, Vector2Int> marks, params string[] rows)
    {
        int sizeZ = rows.Length;
        int sizeX = rows[0].Length;
        var obstacles = new bool[sizeX * sizeZ];
        for (int r = 0; r < sizeZ; r++)
        {
            int z = sizeZ - 1 - r;
            for (int x = 0; x < sizeX; x++)
            {
                char c = rows[r][x];
                if (c == '#') obstacles[z * sizeX + x] = true;
                else if (c != '.') marks[c] = new Vector2Int(x, z);
            }
        }
        return new GridState(sizeX, sizeZ, obstacles);
    }

    public static Vector2Int RandomFreeCell(System.Random rng, GridState grid)
    {
        var free = new List<Vector2Int>();
        for (int z = 0; z < grid.SizeZ; z++)
            for (int x = 0; x < grid.SizeX; x++)
                if (grid.IsWalkable(x, z)) free.Add(new Vector2Int(x, z));
        return free.Count == 0 ? new Vector2Int(-1, -1) : free[rng.Next(free.Count)];
    }

    // Shortest number of steps from start to every reachable walkable cell (start itself: 0).
    // why: BFS is the obviously-correct way to get shortest paths with unit costs, so A* must agree with it.
    public static Dictionary<Vector2Int, int> BfsDistances(GridState grid, Vector2Int start)
    {
        var dist = new Dictionary<Vector2Int, int> { [start] = 0 };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            foreach (Vector2Int dir in TurnRules.Around)
            {
                Vector2Int next = cur + dir;
                if (dist.ContainsKey(next) || !grid.IsWalkable(next.x, next.y)) continue;
                dist[next] = dist[cur] + 1;
                queue.Enqueue(next);
            }
        }
        return dist;
    }

    // Separated: the turn limit came and the enemy has no path to the player at all (walled-off regions).
    public enum Ending { Caught, Trapped, Separated, TurnLimit }

    public class MatchResult
    {
        public Ending Ending;
        public int Turns;
        public readonly List<string> Problems = new List<string>();
    }

    // One match played the way the game plays it: the player (a random bot) walks to a random reachable
    // tile, then the enemy gets as many steps as the player walked. The invariants are checked after
    // every move, as the in-game QA probe does.
    public static MatchResult PlayMatch(System.Random rng, GridState grid, Vector2Int player, Vector2Int enemy,
                                        int turnLimit)
    {
        var result = new MatchResult();
        grid.SetOccupied(player.x, player.y);
        grid.SetOccupied(enemy.x, enemy.y);

        for (int turn = 1; turn <= turnLimit; turn++)
        {
            result.Turns = turn;
            if (!TurnRules.HasAnyMove(grid, player))
            {
                result.Ending = Ending.Trapped;
                return result;
            }

            List<Vector2Int> options = TurnRules.LegalDestinations(grid, player);
            Vector2Int goal = options[rng.Next(options.Count)];
            List<Vector2Int> path = Pathfinder.FindPath(player, goal, grid);
            player = Walk(grid, player, path);

            List<Vector2Int> plan = TurnRules.PlanEnemyTurn(grid, enemy, player, path.Count);
            enemy = Walk(grid, enemy, plan);

            bool adjacent = TurnRules.IsAdjacent(enemy, player);
            var units = new Dictionary<string, Vector2Int> { ["player"] = player, ["enemy"] = enemy };
            result.Problems.AddRange(Invariants.CheckBoard(grid, units));
            result.Problems.AddRange(Invariants.CheckEnemyTurn(path.Count, plan.Count, adjacent, adjacent));
            if (result.Problems.Count > 0) return result;

            if (adjacent)
            {
                result.Ending = Ending.Caught;
                return result;
            }
        }
        result.Ending = TurnRules.PlanEnemyTurn(grid, enemy, player, int.MaxValue).Count == 0
            ? Ending.Separated
            : Ending.TurnLimit;
        return result;
    }

    // Moves a unit along cells, keeping the grid's occupied set the way Unit.StepTo does.
    private static Vector2Int Walk(GridState grid, Vector2Int from, List<Vector2Int> cells)
    {
        foreach (Vector2Int next in cells)
        {
            grid.SetOccupied(next.x, next.y);
            grid.SetFree(from.x, from.y);
            from = next;
        }
        return from;
    }
}
