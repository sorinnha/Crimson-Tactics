# Crimson Tactics

A small turn based tactics prototype on a grid. It has tile generation, A* style
pathfinding, player units you move around, and a basic enemy AI that takes its own
turn to move and attack.

## Run it
Open the folder in Unity 6000.3.16f1 and load the scene in Assets/Scenes.

## Notes
An early prototype, built mostly to practice grid movement and pathfinding. The
code is in Assets/Scripts (grid manager, tile generator, pathfinder, units,
enemy AI, game controller).

## Testing and QA

The rules (A* pathfinding, the enemy's turn, the board) live in plain C# in `Assets/Scripts/Rules`, so they
are tested without Unity: A* is checked against breadth-first search on 20,000 random queries, the enemy's
turn on 2,000 random boards, and whole matches with a random bot on random boards and on the shipped level,
with the game's invariants checked every turn. The same tests run in Unity's Test Runner and in CI
(`dotnet test tools/rules-tests`). What they found and what was fixed: [docs/QA_FINDINGS.md](docs/QA_FINDINGS.md).

`Assets/QA` plugs the game into [Miaw QA Lab](https://github.com/sorinnha/miaw-qa-lab): a bot that plays
through the game's own move command, and a probe that reports any broken rule as a bug with a screenshot.
Steps to run it: [docs/RUN_ON_PC.md](docs/RUN_ON_PC.md).
