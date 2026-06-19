using System.Collections.Generic;
using UnityEngine;

// click a tile to move the player along the path to it
public class PlayerUnit : Unit
{
    // tells the enemy how many tiles we moved so it gets the same amount
    public event System.Action<int> TurnEnded;

    private bool isMyTurn = true;
    private int lastMoveDistance;
    private TileInfo highlightedTile;

    protected override void Start()
    {
        base.Start();

        // turn off root motion so the animation doesn't drag the cube off its tile
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
    }

    void Update()
    {
        if (GameController.Instance.IsGameOver) return;

        if (Grid == null || IsMoving || !isMyTurn) return;

        if (Input.GetMouseButtonDown(0))
            TryMoveToClickedTile();
    }

    private void TryMoveToClickedTile()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        TileInfo tile = hit.collider.GetComponent<TileInfo>();
        if (tile == null) return;

        Vector2Int goal = new Vector2Int(tile.tileX, tile.tileZ);

        List<Vector2Int> path = Pathfinder.FindPath(Cell, goal, Grid);
        if (path.Count == 0) return;

        lastMoveDistance = path.Count;
        HighlightTarget(tile);
        StartCoroutine(MoveAlongPath(path));
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
        TurnEnded?.Invoke(lastMoveDistance);
    }

    public void BeginTurn()
    {
        isMyTurn = true;
    }
}
