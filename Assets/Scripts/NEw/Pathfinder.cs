using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    // 4 directions, no diagonals
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, GridManager grid)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (start == goal) return result;
        if (!grid.IsWalkable(goal.x, goal.y)) return result;

        List<Node> open = new List<Node>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Node> created = new Dictionary<Vector2Int, Node>();

        Node startNode = new Node(start.x, start.y);
        startNode.gCost = 0;
        startNode.hCost = Heuristic(start, goal);
        open.Add(startNode);
        created[start] = startNode;

        while (open.Count > 0)
        {
            // node with the lowest f cost
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].FCost < current.FCost ||
                    (open[i].FCost == current.FCost && open[i].hCost < current.hCost))
                {
                    current = open[i];
                }
            }

            if (current.Cell == goal)
                return Retrace(current);

            open.Remove(current);
            closed.Add(current.Cell);

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int neighbourCell = current.Cell + dir;

                if (closed.Contains(neighbourCell)) continue;
                if (!grid.IsWalkable(neighbourCell.x, neighbourCell.y)) continue;

                int tentativeG = current.gCost + 1;

                Node neighbour;
                if (!created.TryGetValue(neighbourCell, out neighbour))
                {
                    neighbour = new Node(neighbourCell.x, neighbourCell.y);
                    created[neighbourCell] = neighbour;
                }

                bool inOpen = open.Contains(neighbour);

                if (!inOpen || tentativeG < neighbour.gCost)
                {
                    neighbour.gCost = tentativeG;
                    neighbour.hCost = Heuristic(neighbourCell, goal);
                    neighbour.cameFrom = current;

                    if (!inOpen) open.Add(neighbour);
                }
            }
        }

        return result;
    }

    // manhattan distance
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> Retrace(Node goalNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = goalNode;

        while (current.cameFrom != null)
        {
            path.Add(current.Cell);
            current = current.cameFrom;
        }

        path.Reverse();
        return path;
    }
}
