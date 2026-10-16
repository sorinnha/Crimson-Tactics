using System.Collections.Generic;
using UnityEngine;

// click a tile to move the player along the path to it
public class PlayerUnit : Unit
{
    // tells the enemy how many tiles we moved so it gets the same amount
    public event System.Action<int> TurnEnded;
    // our turn has begun (the QA probe checks the board here, when nothing is moving)
    public event System.Action TurnStarted;

    private bool isMyTurn = true;
    private TileInfo highlightedTile;

    // True when a click (or the QA bot) may give an order: our turn, nothing moving, game still on.
    public bool CanAct => isMyTurn && !IsMoving && Grid != null && !GameController.Instance.IsGameOver;

    protected override void Start()
    {
        base.Start();

        // turn off root motion so the animation doesn't drag the cube off its tile
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
    }

    void Update()
    {
        if (CanAct && Input.GetMouseButtonDown(0))
            TryMoveToClickedTile();
    }

    private void TryMoveToClickedTile()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        TileInfo tile = hit.collider.GetComponent<TileInfo>();
        if (tile == null) return;

        if (TryMoveTo(new Vector2Int(tile.tileX, tile.tileZ)))
            HighlightTarget(tile);
    }

    // The one way to give the player an order: a click ends up here, and so does the QA bot.
    // Returns false if it isn't our turn or the tile can't be reached.
    public bool TryMoveTo(Vector2Int goal)
    {
        if (!CanAct) return false;

        List<Vector2Int> path = Pathfinder.FindPath(Cell, goal, Grid.State);
        if (path.Count == 0) return false;

        StartCoroutine(MoveAlongPath(path));
        return true;
    }

    private void HighlightTarget(TileInfo tile)
    {
        if (highlightedTile != null) highlightedTile.ClearHighlight();
        tile.HighlightAsTarget();
        highlightedTile = tile;
    }

    protected override void OnFinishedMoving()
    {
        if (highlightedTile != null)
        {
            highlightedTile.ClearHighlight();
            highlightedTile = null;
        }

        isMyTurn = false;
        // why: the steps really walked, not the planned path length. A path can stop early when a cell
        // gets blocked, and the enemy must not get more moves than the player actually made.
        TurnEnded?.Invoke(StepsWalked);
    }

    public void BeginTurn()
    {
        // why: with no free tile next to us, no click can ever end the turn and the game would hang.
        if (!TurnRules.HasAnyMove(Grid.State, Cell))
        {
            GameController.Instance.GameOver("You're trapped: no tile left to move to!");
            return;
        }

        isMyTurn = true;
        TurnStarted?.Invoke();
    }
}
