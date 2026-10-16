using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// The same checks on the level that ships: Assets/Editor/ObstacleData.asset, player at 0,0 and enemy at
// 9,9 as placed in the scene.
public class RealLevelTests
{
    private static readonly Vector2Int PlayerSpawn = new Vector2Int(0, 0);
    private static readonly Vector2Int EnemySpawn = new Vector2Int(9, 9);

    // Reads gridObstacles from the asset file (Unity writes a bool array as hex bytes, "00" or "01").
    // why: reading the real file means the test breaks if someone redraws the level into a broken one.
    private static GridState LoadLevel()
    {
        string asset = FindUp(Path.Combine("Assets", "Editor", "ObstacleData.asset"));
        foreach (string line in File.ReadAllLines(asset))
        {
            string t = line.Trim();
            if (!t.StartsWith("gridObstacles:")) continue;
            string hex = t.Substring("gridObstacles:".Length).Trim();
            var obstacles = new bool[hex.Length / 2];
            for (int i = 0; i < obstacles.Length; i++) obstacles[i] = hex.Substring(2 * i, 2) != "00";
            return new GridState(10, 10, obstacles);
        }
        throw new AssertionException("no gridObstacles in " + asset);
    }

    private static string FindUp(string relative)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (; dir != null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException(relative);
    }

    [Test]
    public void EveryFreeCellIsReachable_SoNoMatchCanBeEndless()
    {
        GridState grid = LoadLevel();
        Assert.That(grid.IsWalkable(PlayerSpawn.x, PlayerSpawn.y), "player spawns on an obstacle");
        Assert.That(grid.IsWalkable(EnemySpawn.x, EnemySpawn.y), "enemy spawns on an obstacle");

        Dictionary<Vector2Int, int> reach = TestBoards.BfsDistances(grid, PlayerSpawn);
        var unreachable = new List<string>();
        for (int z = 0; z < 10; z++)
            for (int x = 0; x < 10; x++)
                if (grid.IsWalkable(x, z) && !reach.ContainsKey(new Vector2Int(x, z))) unreachable.Add($"{x},{z}");
        Assert.That(unreachable, Is.Empty, "cells the player can never reach");
    }

    [Test]
    public void ThousandRandomMatchesOnTheRealLevelBreakNoRule()
    {
        var rng = new System.Random(11);
        var endings = new Dictionary<TestBoards.Ending, int>();
        var turns = new List<int>();
        for (int match = 0; match < 1000; match++)
        {
            TestBoards.MatchResult r = TestBoards.PlayMatch(rng, LoadLevel(), PlayerSpawn, EnemySpawn, 200);
            Assert.That(r.Problems, Is.Empty, $"match {match}, turn {r.Turns}");
            endings[r.Ending] = endings.TryGetValue(r.Ending, out int n) ? n + 1 : 1;
            turns.Add(r.Turns);
        }
        turns.Sort();
        TestContext.WriteLine("endings: " + string.Join(", ", endings) + $"; median turns {turns[turns.Count / 2]}");
        Assert.That(endings.ContainsKey(TestBoards.Ending.Separated), Is.False);
    }
}
