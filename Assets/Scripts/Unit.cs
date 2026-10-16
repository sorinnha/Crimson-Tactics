using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for the player and enemy, moves one tile at a time
public abstract class Unit : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    public int CellX { get; protected set; }
    public int CellZ { get; protected set; }
    public Vector2Int Cell => new Vector2Int(CellX, CellZ);

    public bool IsMoving { get; protected set; }

    protected GridManager Grid => GridManager.Instance;
    private float restingHeight;

    protected virtual void Start()
    {
        restingHeight = transform.position.y;
        SnapToStartCell();
    }

    private void SnapToStartCell()
    {
        Vector2Int cell = Grid.WorldToCell(transform.position);
        if (!Grid.IsWalkable(cell.x, cell.y))
            cell = FindNearestFreeCell(cell);

        CellX = cell.x;
        CellZ = cell.y;
        transform.position = Grid.CellToWorld(CellX, CellZ, restingHeight);
        Grid.SetOccupied(CellX, CellZ);
    }

    private Vector2Int FindNearestFreeCell(Vector2Int from)
    {
        int maxRadius = Mathf.Max(Grid.gridSizeX, Grid.gridSizeZ);
        for (int r = 1; r <= maxRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    int x = from.x + dx;
                    int z = from.y + dz;
                    if (Grid.IsWalkable(x, z)) return new Vector2Int(x, z);
                }
            }
        }
        return from;
    }

    protected bool IsStepClear(Vector2Int cell)
    {
        if (!Grid.IsInsideGrid(cell.x, cell.y)) return false;
        if (Grid.IsObstacle(cell.x, cell.y)) return false;
        if (Grid.IsOccupied(cell.x, cell.y) && cell != Cell) return false;
        return true;
    }

    // How many cells the last MoveAlongPath really walked (it stops early if a cell gets blocked).
    public int StepsWalked { get; private set; }

    // walk the whole path, stop if a cell gets blocked on the way
    protected IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        StepsWalked = 0;
        if (path == null || path.Count == 0) yield break;

        IsMoving = true;
        foreach (Vector2Int step in path)
        {
            if (!IsStepClear(step)) break;
            yield return StepTo(step);
            StepsWalked++;
        }
        IsMoving = false;

        OnFinishedMoving();
    }

    protected IEnumerator StepOnce(Vector2Int next)
    {
        if (!IsStepClear(next)) yield break;

        IsMoving = true;
        yield return StepTo(next);
        IsMoving = false;
    }

    private IEnumerator StepTo(Vector2Int next)
    {
        Vector3 from = transform.position;
        Vector3 to = Grid.CellToWorld(next.x, next.y, restingHeight);
        float duration = Vector3.Distance(from, to) / Mathf.Max(moveSpeed, 0.01f);

        int oldX = CellX;
        int oldZ = CellZ;
        Grid.SetOccupied(next.x, next.y); // reserve the cell we are moving into

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.0001f);
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }

        transform.position = to;
        CellX = next.x;
        CellZ = next.y;
        Grid.SetFree(oldX, oldZ); // free the old cell
    }

    protected virtual void OnFinishedMoving() { }
}
