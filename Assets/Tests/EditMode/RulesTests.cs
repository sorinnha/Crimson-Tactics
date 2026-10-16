using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PathfinderTests
{
    [Test]
    public void SameStartAndGoal_GivesNoSteps()
    {
        var grid = new GridState(5, 5, null);
        Assert.That(Pathfinder.FindPath(new Vector2Int(2, 2), new Vector2Int(2, 2), grid), Is.Empty);
    }

    [Test]
    public void GoalOnObstacleOrUnreachable_GivesNoPath()
    {
        var m = new Dictionary<char, Vector2Int>();
        GridState grid = TestBoards.Parse(m,
            "S.#.",
            "..#G",
            "###.",
            "....");
        Assert.That(Pathfinder.FindPath(m['S'], new Vector2Int(2, 3), grid), Is.Empty, "goal is an obstacle");
        Assert.That(Pathfinder.FindPath(m['S'], m['G'], grid), Is.Empty, "goal is walled off");
    }

    [Test]
    public void GoesAroundAWall()
    {
        var m = new Dictionary<char, Vector2Int>();
        GridState grid = TestBoards.Parse(m,
            "S#G",
            ".#.",
            "...");
        List<Vector2Int> path = Pathfinder.FindPath(m['S'], m['G'], grid);
        Assert.That(path.Count, Is.EqualTo(6));
        Assert.That(path[path.Count - 1], Is.EqualTo(m['G']));
    }

    // The main property: on thousands of random boards, A* finds a path exactly when one exists, the
    // path is legal step by step, and it is as short as breadth-first search says the shortest is.
    [Test]
    public void AgreesWithBreadthFirstSearchOnRandomBoards()
    {
        var rng = new System.Random(20261016);
        int queries = 0;
        for (int board = 0; board < 2000; board++)
        {
            GridState grid = TestBoards.Random(rng, 10, 10, 0.1 + 0.3 * rng.NextDouble());
            Vector2Int start = TestBoards.RandomFreeCell(rng, grid);
            if (start.x < 0) continue;
            Dictionary<Vector2Int, int> dist = TestBoards.BfsDistances(grid, start);

            for (int q = 0; q < 10; q++, queries++)
            {
                var goal = new Vector2Int(rng.Next(10), rng.Next(10));
                List<Vector2Int> path = Pathfinder.FindPath(start, goal, grid);
                string where = $"board {board}, {start} -> {goal}";

                if (goal == start || !dist.ContainsKey(goal))
                {
                    Assert.That(path, Is.Empty, where);
                    continue;
                }
                Assert.That(path.Count, Is.EqualTo(dist[goal]), "not the shortest path: " + where);
                AssertLegalPath(grid, start, goal, path, where);
            }
        }
        TestContext.WriteLine($"A* agreed with BFS on {queries} queries over 2000 boards");
        Assert.That(queries, Is.GreaterThan(15000));
    }

    private static void AssertLegalPath(GridState grid, Vector2Int start, Vector2Int goal, List<Vector2Int> path,
                                        string where)
    {
        Vector2Int prev = start;
        foreach (Vector2Int cell in path)
        {
            Assert.That(TurnRules.IsAdjacent(prev, cell), "jump in path: " + where);
            Assert.That(grid.IsWalkable(cell.x, cell.y), "path crosses a blocked cell: " + where);
            prev = cell;
        }
        Assert.That(prev, Is.EqualTo(goal), where);
    }
}

public class TurnRulesTests
{
    [Test]
    public void ABoxedInUnitHasNoMove()
    {
        var m = new Dictionary<char, Vector2Int>();
        GridState grid = TestBoards.Parse(m,
            ".#.",
            "#P#",
            ".#.");
        Assert.That(TurnRules.HasAnyMove(grid, m['P']), Is.False);
        Assert.That(TurnRules.LegalDestinations(grid, m['P']), Is.Empty);
    }

    [Test]
    public void LegalDestinationsAreExactlyTheReachableCells()
    {
        var rng = new System.Random(7);
        for (int board = 0; board < 300; board++)
        {
            GridState grid = TestBoards.Random(rng, 10, 10, 0.25);
            Vector2Int from = TestBoards.RandomFreeCell(rng, grid);
            if (from.x < 0) continue;
            var expected = new HashSet<Vector2Int>(TestBoards.BfsDistances(grid, from).Keys);
            expected.Remove(from);
            Assert.That(new HashSet<Vector2Int>(TurnRules.LegalDestinations(grid, from)), Is.EquivalentTo(expected));
        }
    }

    [Test]
    public void EnemyStopsAsSoonAsItIsNextToThePlayer()
    {
        var m = new Dictionary<char, Vector2Int>();
        GridState grid = TestBoards.Parse(m,
            "E....P");
        grid.SetOccupied(m['E'].x, m['E'].y);
        grid.SetOccupied(m['P'].x, m['P'].y);
        List<Vector2Int> plan = TurnRules.PlanEnemyTurn(grid, m['E'], m['P'], 10);
        Assert.That(plan.Count, Is.EqualTo(4));
        Assert.That(TurnRules.IsAdjacent(plan[plan.Count - 1], m['P']));
    }

    [Test]
    public void EnemyOnRandomBoards_KeepsToItsBudgetAndOnlyWalksFreeCells()
    {
        var rng = new System.Random(99);
        for (int board = 0; board < 2000; board++)
        {
            GridState grid = TestBoards.Random(rng, 10, 10, 0.25);
            Vector2Int player = TestBoards.RandomFreeCell(rng, grid);
            if (player.x < 0) continue;
            grid.SetOccupied(player.x, player.y);
            Vector2Int enemy = TestBoards.RandomFreeCell(rng, grid);
            if (enemy.x < 0) continue;
            grid.SetOccupied(enemy.x, enemy.y);
            int budget = rng.Next(1, 12);

            List<Vector2Int> plan = TurnRules.PlanEnemyTurn(grid, enemy, player, budget);
            Assert.That(plan.Count, Is.LessThanOrEqualTo(budget));

            Vector2Int prev = enemy;
            foreach (Vector2Int cell in plan)
            {
                Assert.That(TurnRules.IsAdjacent(prev, cell));
                Assert.That(grid.IsWalkable(cell.x, cell.y), $"board {board}: enemy walks onto {cell}");
                prev = cell;
            }

            // If some free tile next to the player is within the budget, the enemy must get there.
            int closest = int.MaxValue;
            Dictionary<Vector2Int, int> dist = TestBoards.BfsDistances(grid, enemy);
            foreach (Vector2Int dir in TurnRules.Around)
                if (dist.TryGetValue(player + dir, out int d)) closest = System.Math.Min(closest, d);
            bool startsNext = TurnRules.IsAdjacent(enemy, player);
            if (!startsNext && closest <= budget)
                Assert.That(TurnRules.IsAdjacent(prev, player), $"board {board}: enemy could have caught the player");
        }
    }
}

public class InvariantTests
{
    [Test]
    public void FindsSharedCellsLeakedReservationsAndUnitsOnObstacles()
    {
        var obstacles = new bool[9];
        obstacles[4] = true; // cell 1,1
        var grid = new GridState(3, 3, obstacles);
        grid.SetOccupied(0, 0);
        grid.SetOccupied(2, 2); // nobody stands here: a leaked reservation
        var units = new Dictionary<string, Vector2Int>
        {
            ["player"] = new Vector2Int(0, 0),
            ["enemy"] = new Vector2Int(0, 0),
        };
        List<string> problems = Invariants.CheckBoard(grid, units);
        Assert.That(problems, Has.Some.Contains("share cell 0,0"));
        Assert.That(problems, Has.Some.Contains("2,2 is marked occupied but nobody stands there"));

        var onRock = new Dictionary<string, Vector2Int> { ["enemy"] = new Vector2Int(1, 1) };
        grid.SetOccupied(1, 1);
        Assert.That(Invariants.CheckBoard(grid, onRock), Has.Some.Contains("obstacle"));
    }

    [Test]
    public void FlagsAnEnemyThatOutWalksThePlayer()
    {
        Assert.That(Invariants.CheckEnemyTurn(3, 5, false, false), Has.Some.Contains("enemy walked 5 steps"));
        Assert.That(Invariants.CheckEnemyTurn(3, 3, true, false), Has.Some.Contains("not over"));
        Assert.That(Invariants.CheckEnemyTurn(3, 3, true, true), Is.Empty);
    }
}

public class SimulatedMatchTests
{
    // 1,000 whole matches with a random player on random boards: no rule may ever break, and every match
    // ends (caught, trapped, or still running at the turn limit, never stuck).
    [Test]
    public void ThousandRandomMatchesBreakNoRule()
    {
        var rng = new System.Random(5);
        var endings = new Dictionary<TestBoards.Ending, int>();
        for (int match = 0; match < 1000; match++)
        {
            GridState grid = TestBoards.Random(rng, 10, 10, 0.2);
            Vector2Int player = TestBoards.RandomFreeCell(rng, grid);
            grid.SetOccupied(player.x, player.y);
            Vector2Int enemy = TestBoards.RandomFreeCell(rng, grid);
            grid.SetFree(player.x, player.y);
            if (player.x < 0 || enemy.x < 0 || TurnRules.IsAdjacent(player, enemy)) continue;

            TestBoards.MatchResult r = TestBoards.PlayMatch(rng, grid, player, enemy, 200);
            Assert.That(r.Problems, Is.Empty, $"match {match}, turn {r.Turns}");
            endings[r.Ending] = endings.TryGetValue(r.Ending, out int n) ? n + 1 : 1;
        }
        TestContext.WriteLine("endings: " + string.Join(", ", endings));
        Assert.That(endings.ContainsKey(TestBoards.Ending.Caught), "a chase that never catches anyone is suspicious");
    }
}
