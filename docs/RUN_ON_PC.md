# Steps on your PC (Unity 6000.3.16f1)

Nothing in `Assets/Scripts` changed what the game does on the shipped level, except the new
"You're trapped" ending. Check it still plays, then run the tests, then the QA Lab bot.

## 1. Open and play

1. Open the project in Unity Hub (6000.3.16f1). Unity creates `.meta` files for the new folders and the
   two assembly definitions (`Assets/Scripts/Rules`, `Assets/Tests/EditMode`). Commit those `.meta` files.
2. The Console must have no errors. Open `Assets/Scenes/Crimson Tactics.unity`, press Play, click tiles:
   the player walks, the enemy follows with as many steps, "GAME OVER - The enemy caught you!" when it
   reaches you, R restarts.

## 2. Run the tests in Unity

**Window → General → Test Runner → EditMode → Run All.** Expect 13 passed (the same 13 as
`dotnet test tools/rules-tests`). Without Unity, from the repo root:

```powershell
dotnet test tools/rules-tests -c Release --logger "console;verbosity=normal"
dotnet build tools/compile-check -c Release
dotnet build tools/compile-check-qa -c Release
```

## 3. Let the QA Lab bot play

1. **Window → Package Manager → + → Add package from git URL…**
   `https://github.com/sorinnha/miaw-qa-lab.git?path=/unity/com.miawworks.qalab`
   (the repo is private: Git on your PC must be signed in to GitHub).
2. **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols:** add `QALAB`, Apply.
   The Console must stay free of errors (first real compile of `Assets/QA`).
3. **File → Build Profiles:** make sure `Crimson Tactics` is in the scene list (the probe restarts the scene
   by its build index).
4. **Tools → QA Lab → Window:** Adapter `crimson_tactics`, Out Dir `../qalab-runs`, Duration 300,
   **Play with QA Lab.** You should see the player move by itself, matches restart after each game over,
   and a run folder appear when it stops.
5. Check and triage the run (from the miaw-qa-lab repo, venv active):

   ```powershell
   qalab validate ..\qalab-runs\<run folder>
   qalab triage run ..\qalab-runs\<run folder> --provider none --repo <crimson-tactics>\Assets --out out\crimson_report
   ```

   Expected on the shipped level: `order` actions for every move and no `rule_broken` events. To see the
   probe catch something, break a rule on purpose (for example give the enemy `maxSteps + 1` in
   `EnemyAI.TakeTurn`), run again, and check that `rule_broken` "enemy walked N steps after the player
   walked M" shows up in the report with a screenshot. Undo the change afterwards.
6. Write what you saw (runs, matches, events, anything surprising) into `docs/QA_FINDINGS.md`, with the
   numbers from the run folder only.
