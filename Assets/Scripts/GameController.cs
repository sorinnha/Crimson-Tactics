using UnityEngine;
using UnityEngine.SceneManagement;

// tracks whether the game is over and draws the game over screen
public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameController>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameController (auto)");
                    _instance = go.AddComponent<GameController>();
                }
            }
            return _instance;
        }
    }

    public bool IsGameOver { get; private set; }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Debug.Log("GAME OVER - the enemy caught the player!");
    }

    void Update()
    {
        // press R to restart after losing
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            if (index >= 0) SceneManager.LoadScene(index);
        }
    }

    void OnGUI()
    {
        if (!IsGameOver) return;

        // dim the screen
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle big = new GUIStyle
        {
            fontSize = 64,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        big.normal.textColor = Color.red;
        GUI.Label(new Rect(0, Screen.height * 0.5f - 80, Screen.width, 80), "GAME OVER", big);

        GUIStyle small = new GUIStyle
        {
            fontSize = 26,
            alignment = TextAnchor.MiddleCenter
        };
        small.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.5f + 10, Screen.width, 40),
            "The enemy caught you!", small);
        GUI.Label(new Rect(0, Screen.height * 0.5f + 50, Screen.width, 40),
            "Press R to play again", small);
    }
}
