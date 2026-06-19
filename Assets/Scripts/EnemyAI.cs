using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enemy chases the player and gets the same number of moves the player just made
public class EnemyAI : Unit, IAI
{
    private PlayerUnit player;
    private int moveBudget;

    private static readonly Vector2Int[] Around =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    protected override void Start()
    {
        base.Start();
        player = FindFirstObjectByType<PlayerUnit>();

        if (player != null) player.TurnEnded += OnPlayerTurnEnded;
    }

    void OnDestroy()
    {
        if (player != null) player.TurnEnded -= OnPlayerTurnEnded;
    }

    private void OnPlayerTurnEnded(int playerSteps)
    {
        moveBudget = playerSteps;
        Think();
    }

    public void Think()
    {
        StartCoroutine(TakeTurn(moveBudget));
    }

    private IEnumerator TakeTurn(int maxSteps)
    {
        if (player != null && !IsAdjacentTo(player.Cell))
        {
            Vector2Int? target = PickClosestTileNextToPlayer();
            if (target != null)
            {
                List<Vector2Int> path = Pathfinder.FindPath(Cell, target.Value, Grid);
                int steps = Mathf.Min(maxSteps, path.Count);

                for (int i = 0; i < steps; i++)
                {
                    if (!IsStepClear(path[i])) break;
                    yield return StepOnce(path[i]);
                    if (IsAdjacentTo(player.Cell)) break;
                }
            }
        }

        // standing next to the player means we caught them
        if (player != null && IsAdjacentTo(player.Cell))
            GameController.Instance.GameOver();
        else
            EndTurn();
    }

    private void EndTurn()
    {
        if (player != null) player.BeginTurn();
    }

    // closest reachable tile next to the player
    private Vector2Int? PickClosestTileNextToPlayer()
    {
        Vector2Int? best = null;
        int bestLength = int.MaxValue;

        foreach (Vector2Int offset in Around)
        {
            Vector2Int cell = player.Cell + offset;
            if (!Grid.IsWalkable(cell.x, cell.y)) continue;

            List<Vector2Int> path = Pathfinder.FindPath(Cell, cell, Grid);
            if (path.Count == 0) continue;

            if (path.Count < bestLength)
            {
                bestLength = path.Count;
                best = cell;
            }
        }

        return best;
    }

    private bool IsAdjacentTo(Vector2Int cell)
    {
        int distance = Mathf.Abs(Cell.x - cell.x) + Mathf.Abs(Cell.y - cell.y);
        return distance == 1;
    }
}
