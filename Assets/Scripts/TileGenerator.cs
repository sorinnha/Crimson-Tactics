using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tilePrefab; // Prefab for the tile
    public int tileSizeX = 10; // Size of the tile in the X direction
    public int tileSizeZ = 10; // Size of the tile in the Z direction
    
    void Start()
    {
        CreateTileGrid(); 
    }
    
    void CreateTileGrid()
    {
        for (int x = 0; x < tileSizeX ; x++) // loop for x coordinates
        {
            for (int z = 0; z < tileSizeZ ; z++) // loop for z coordinates
            {
                Vector3 position = new Vector3(x , 0, z); // create position for the cube
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity); // create the cube at that position using above position
                TileInfo tileInfo = newTile.GetComponent<TileInfo>(); // get tileInfo
                tileInfo.SetTilePosition(x, z); // use settileposition function from TileInfo 
            }
        }
    }
}
