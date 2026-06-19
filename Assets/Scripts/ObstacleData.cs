using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Obstacle/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    public bool[] gridObstacles = new bool[100]; // made a list of 100 bool to check if its an obstacle or not
}
