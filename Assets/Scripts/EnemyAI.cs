using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enemy chases the player and gets the same number of moves the player just made
public class EnemyAI : Unit, IAI
{
    private PlayerUnit player;
    private int moveBudget;

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

    // How far the enemy walked on its last turn (the QA probe checks it never beats the player's).
    public int LastSteps { get; private set; }

    private IEnumerator TakeTurn(int maxSteps)
    {
        LastSteps = 0;
        if (player != null)
        {
            // the plan comes from TurnRules, which the tests check over thousands of boards
            List<Vector2Int> plan = TurnRules.PlanEnemyTurn(Grid.State, Cell, player.Cell, maxSteps);
            foreach (Vector2Int step in plan)
            {
                if (!IsStepClear(step)) break;
                yield return StepOnce(step);
                LastSteps++;
            }
        }

        // standing next to the player means we caught them
        if (player != null && TurnRules.IsAdjacent(Cell, player.Cell))
            GameController.Instance.GameOver("The enemy caught you!");
        else
            EndTurn();
    }

    private void EndTurn()
    {
        if (player != null) player.BeginTurn();
    }
}
