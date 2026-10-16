#if QALAB
using System;
using System.Collections.Generic;
using MiawWorks.QALab;

namespace CrimsonTactics.QA
{
    /// <summary>
    /// The QA bot for Crimson Tactics: each step it asks <see cref="TurnPolicy.Decide"/> for a move, gives it
    /// through <see cref="IGameCommands"/> (the same <c>PlayerUnit.TryMoveTo</c> a click calls) and logs it,
    /// so a bug report's "steps to reproduce" read like "move 3,4", "move 7,1".
    /// Run with <c>-qalab -qalabAdapter crimson_tactics</c>.
    /// </summary>
    public sealed class CrimsonTacticsAdapter : IBotAdapter
    {
        public const string AdapterName = "crimson_tactics";

        private readonly Func<IGameCommands> _findGame;
        private int _ordersThisTurn;

        public CrimsonTacticsAdapter(Func<IGameCommands> findGame)
        {
            _findGame = findGame ?? throw new ArgumentNullException(nameof(findGame));
        }

        public string Name => AdapterName;

        public void Begin(BotContext ctx) => _ordersThisTurn = 0;

        public BotStepResult Step(BotContext ctx)
        {
            // why: looked up every step, because a restart reloads the scene and destroys the old probe
            var game = _findGame();
            var decision = TurnPolicy.Decide(game, ctx.Random, _ordersThisTurn);
            switch (decision.Kind)
            {
                case BotDecisionKind.EndTurn:
                    game.EndTurn();
                    _ordersThisTurn = 0;
                    ctx.LogAction("end_turn");
                    break;
                case BotDecisionKind.Order:
                    bool accepted = game.Issue(decision.Unit, decision.Order);
                    // A move ends the player's turn by itself, so each accepted order starts a fresh count.
                    _ordersThisTurn = accepted ? 0 : _ordersThisTurn + 1;
                    ctx.LogAction("order", new Dictionary<string, object>
                    {
                        ["unit"] = decision.Unit, ["order"] = decision.Order, ["accepted"] = accepted,
                    });
                    break;
            }
            return BotStepResult.Continue;
        }

        public void End(BotContext ctx) => _ordersThisTurn = 0;
    }
}
#endif
