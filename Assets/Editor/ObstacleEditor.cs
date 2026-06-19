using UnityEngine;
using UnityEditor;

public class ObstacleEditor : EditorWindow
{
    // our obstacle data file reference
    private ObstacleData obstacleData;
    
    // this makes the menu item appear
    [MenuItem("Tools/Obstacle Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleEditor>("Obstacle Editor");
    }
    
    // this draws our window
    void OnGUI()
    {
        // window title
        GUILayout.Label("Obstacle Tool");
        
        // place to drag our data file
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField("Obstacle Data", obstacleData, typeof(ObstacleData), false);
        
        // if no data file, show message
        if (obstacleData == null)
        {
            GUILayout.Label("Please drag GameObstacles file here");
            return;
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Click to toggle obstacles (Red = Obstacle)");
        
        // make 10x10 buttons
        for (int row = 9; row >= 0; row--) // start from top
        {
            GUILayout.BeginHorizontal(); // start new row
            
            for (int col = 0; col < 10; col++)
            {
                // find position in our array
                int arrayPosition = row * 10 + col;
                
                // check if this spot has obstacle
                bool hasObstacle = obstacleData.gridObstacles[arrayPosition];
                
                // color button red if obstacle
                if (hasObstacle)
                    GUI.backgroundColor = Color.red;
                else
                    GUI.backgroundColor = Color.white;
                
                // make button with coordinates
                string buttonText = (col + "," + row);
                if (GUILayout.Button(buttonText, GUILayout.Width(35), GUILayout.Height(25)))
                {
                    // flip obstacle on/off when clicked
                    obstacleData.gridObstacles[arrayPosition] = !hasObstacle;
                    EditorUtility.SetDirty(obstacleData); // save changes
                }
            }
            
            GUILayout.EndHorizontal(); // end row
        }
        
        // reset color back to normal
        GUI.backgroundColor = Color.white;
    }
}
