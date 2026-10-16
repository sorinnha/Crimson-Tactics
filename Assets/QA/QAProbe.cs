#if QALAB
using System.Collections.Generic;
using MiawWorks.QALab;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrimsonTactics.QA
{
    /// <summary>
    /// Lives in the game scene while a QA Lab run is recording. It is the bot's way into the game
    /// (<see cref="IGameCommands"/>), and it checks the rules after every turn with the same
    /// <see cref="Invariants"/> the headless tests use. A broken rule becomes a QA Lab detector event with a
    /// screenshot, which triage turns into a bug report.
    /// </summary>
    public sealed class QAProbe : MonoBehaviour, IGameCommands
    {
        // A match longer than this is reported: the enemy should catch a wandering player long before.
        private const int EndlessTurns = 100;
        private const float RestartDelay = 1f;

        private PlayerUnit player;
        private EnemyAI enemy;
        private int playerSteps;
        private int turn;
        private bool reportedEnd;
        private float restartAt = -1f;

        void Start()
        {
            player = FindFirstObjectByType<PlayerUnit>();
            enemy = FindFirstObjectByType<EnemyAI>();
            if (player == null) return;

            player.TurnEnded += steps => playerSteps = steps;
            player.TurnStarted += OnPlayerTurnStarted;
            GameCommandsLocator.Current = this;
        }

        void OnDestroy()
        {
            if (ReferenceEquals(GameCommandsLocator.Current, this)) GameCommandsLocator.Current = null;
        }

        void Update()
        {
            if (player == null || !GameController.Instance.IsGameOver) return;

            if (!reportedEnd)
            {
                reportedEnd = true;
                bool adjacent = enemy != null && TurnRules.IsAdjacent(enemy.Cell, player.Cell);
                bool trapped = GameController.Instance.Reason.StartsWith("You're trapped");
                // why: a trapped ending happens on the player's turn, before the enemy moves, so it isn't
                // an enemy-turn result and isn't checked as one.
                if (!trapped)
                    Report(Invariants.CheckEnemyTurn(playerSteps, enemy != null ? enemy.LastSteps : 0, adjacent, true));
                restartAt = Time.unscaledTime + RestartDelay;
            }

            // Keep playing: a bot run is many matches back to back, not one.
            if (restartAt > 0f && Time.unscaledTime >= restartAt)
            {
                restartAt = -1f;
                int index = SceneManager.GetActiveScene().buildIndex;
                if (index >= 0) SceneManager.LoadScene(index); // -1: the scene isn't in Build Settings
            }
        }

        private void OnPlayerTurnStarted()
        {
            // why: TurnStarted fires when the enemy hands the turn back, so every call follows an enemy turn
            turn++;
            var problems = new List<string>();
            if (enemy != null)
            {
                bool adjacent = TurnRules.IsAdjacent(enemy.Cell, player.Cell);
                problems.AddRange(Invariants.CheckEnemyTurn(playerSteps, enemy.LastSteps, adjacent, false));
            }
            problems.AddRange(Invariants.CheckBoard(GridManager.Instance.State, Units()));
            Report(problems);

            if (turn == EndlessTurns)
            {
                QALab.ReportDetector("match_endless", DetectorSeverity.Major, new JObject
                {
                    ["turns"] = turn,
                    ["player"] = $"{player.Cell.x},{player.Cell.y}",
                    ["enemy"] = enemy != null ? $"{enemy.Cell.x},{enemy.Cell.y}" : "none",
                });
            }
        }

        private Dictionary<string, Vector2Int> Units()
        {
            var units = new Dictionary<string, Vector2Int> { ["player"] = player.Cell };
            if (enemy != null) units["enemy"] = enemy.Cell;
            return units;
        }

        private void Report(List<string> problems)
        {
            foreach (string problem in problems)
            {
                QALab.ReportDetector("rule_broken", DetectorSeverity.Critical, new JObject
                {
                    ["rule"] = problem,
                    ["turn"] = turn,
                }, player.transform.position);
            }
        }

        // IGameCommands: the bot gives orders through the same method a click uses.

        public bool CanAct => player != null && player.CanAct;

        public IReadOnlyList<string> ActiveUnits() => CanAct ? new[] { "Player" } : new string[0];

        public IReadOnlyList<string> LegalOrders(string unit)
        {
            var orders = new List<string>();
            if (!CanAct || unit != "Player") return orders;
            foreach (Vector2Int cell in TurnRules.LegalDestinations(GridManager.Instance.State, player.Cell))
                orders.Add($"move {cell.x},{cell.y}");
            return orders;
        }

        public bool Issue(string unit, string order)
        {
            if (unit != "Player" || !order.StartsWith("move ")) return false;
            string[] xz = order.Substring(5).Split(',');
            if (xz.Length != 2 || !int.TryParse(xz[0], out int x) || !int.TryParse(xz[1], out int z)) return false;
            return player.TryMoveTo(new Vector2Int(x, z));
        }

        // The turn ends when the move finishes; there is nothing extra to press.
        public void EndTurn() { }
    }
}
#endif
