# What testing found in Crimson Tactics

The game's rules (A* pathfinding, the enemy's turn, the board) were moved out of the MonoBehaviours into
plain C# (`Assets/Scripts/Rules`) so they can be tested without opening Unity. The tests are in
`Assets/Tests/EditMode`; Unity's Test Runner and `dotnet test tools/rules-tests` run the same files.
Every number below is printed by those tests (fixed seeds, so they are the same on every run).

## Checks and results

| Check | Result |
|---|---|
| A* against breadth-first search, random 10×10 boards (10–40% obstacles) | agreed on all **20,000** queries over 2,000 boards: same reachability, same length, every step legal |
| Legal destinations = cells BFS can reach | 300 random boards, all equal |
| Enemy turn: never over its step budget, never onto a blocked cell, catches the player when it can | 2,000 random boards, no failure |
| Whole matches by a random player bot on random boards, invariants checked every turn | **966** matches: 924 caught, 37 *separated* (enemy can never reach the player), 5 *trapped*; 0 rules broken |
| The shipped level (`Assets/Editor/ObstacleData.asset`, player 0,0, enemy 9,9) | every free cell reachable; 1,000 random-bot matches: all caught (median 2 turns), 0 rules broken |

Your A* was right: it is optimal on every board tried. The problems were around it.

## Bugs fixed

| # | Bug | When it bites | Fix |
|---|---|---|---|
| 1 | **Soft-lock when the player can't move.** With no free tile next to the player, no click can end the turn, so the game waits forever. | 5 of 966 random matches (the player started in a sealed pocket). Not on the shipped level. | `PlayerUnit.BeginTurn` ends the game ("You're trapped") instead of waiting forever. |
| 2 | **The enemy's move budget was the planned path length, not the steps actually walked.** `MoveAlongPath` stops early if a cell gets blocked, but the enemy still got the full count. | Latent: nothing moves during the player's walk today, so a path can't get blocked midway. It bites the day anything does (a second enemy, a moving hazard). | `Unit.StepsWalked`; the turn ends with the real count. The in-game probe checks `enemy steps ≤ player steps` every turn. |
| 3 | **`ObstacleManager` hard-coded a 10×10 board** while `GridManager` has a size setting. | Any other board size: obstacles drawn on one cell, blocked on another (the two use different row widths). | It reads the size from `GridManager` and skips cells the data doesn't cover. `ObstacleEditor` and `ObstacleData` (100 cells) are still 10×10: change them together if the board grows. |
| 4 | **`GridManager.IsObstacle` read outside the board:** for x = 10 it returned the obstacle at 0,1 of the next row. | Latent: callers check "inside the grid" first today. | `GridState.IsObstacle` returns false outside the board. |

## Found, not fixed (a design decision for you)

**Separated boards never end.** If obstacles cut the board in two with the player and the enemy on
different sides, the enemy has no path, never moves, and the match goes on forever (37 of 966 random
matches). The shipped level is connected, and `RealLevelTests.EveryFreeCellIsReachable...` now fails if
someone redraws it into two parts, so this can't ship by accident. If levels ever get generated, decide
what should happen (no path → enemy teleports, or player wins, or the generator rejects the level).

## In the running game: QA Lab

`Assets/QA` connects the game to [Miaw QA Lab](https://github.com/sorinnha/miaw-qa-lab). It only
compiles when the `QALAB` scripting define is set, so the game builds normally without the package.

- `crimson_tactics` bot adapter: plays the player's turns through `PlayerUnit.TryMoveTo` (the method a
  click calls) with QA Lab's seeded random, and logs each move as a repro step ("move 3,4").
- `QAProbe`: after every turn checks the same `Invariants` the tests use (units on legal, distinct cells;
  the grid's occupied set matches the units; enemy steps ≤ player steps; game over exactly when caught)
  and reports a broken one as a `rule_broken` detector event (critical, with a screenshot). A match past
  100 turns is reported as `match_endless`. After a game over it restarts the scene, so one run plays
  many matches.

It has not run in Unity yet: the steps are in `docs/RUN_ON_PC.md`. CI compiles it against stubs of
Unity's and QA Lab's APIs (`tools/compile-check-qa`), which catches typos and wrong signatures, not
runtime behaviour.
