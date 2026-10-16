using UnityEngine;

public class Node
{
    public int x;
    public int z;

    public Node cameFrom;
    public int gCost; // steps from the start
    public int hCost; // estimated steps to the goal

    public int FCost => gCost + hCost;

    public Vector2Int Cell => new Vector2Int(x, z);

    public Node(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
