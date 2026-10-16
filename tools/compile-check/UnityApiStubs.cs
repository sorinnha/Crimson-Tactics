// Just enough of the Unity API for the game's own scripts to compile in plain .NET.
// why: Unity isn't installed on CI machines; this catches typos and wrong signatures in the game code.
// Bodies do nothing; only the signatures matter. Add a member here when the game starts using it.
#pragma warning disable CS0067, CS0649, CS8618
using System.Collections;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => original;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o);
        public override int GetHashCode() => 0;
        public static implicit operator bool(Object o) => o != null;
    }
    public class Component : Object
    {
        public Transform transform;
        public GameObject gameObject;
        public T GetComponent<T>() => default;
        public T GetComponentInChildren<T>() => default;
    }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour { public Coroutine StartCoroutine(IEnumerator routine) => null; }
    public class ScriptableObject : Object { }
    public class GameObject : Object
    {
        public GameObject(string name) { }
        public T AddComponent<T>() where T : Component => default;
        public T GetComponent<T>() => default;
        public static GameObject Find(string name) => null;
    }
    public class Transform : Component { public Vector3 position; }
    public class Coroutine { }
    public class Renderer : Component { public Material material; }
    public class Collider : Component { }
    public class Animator : Behaviour { public bool applyRootMotion; }
    public class Material : Object { }
    public class Texture2D : Object { public static Texture2D whiteTexture; }
    public class Camera : Behaviour { public static Camera main; public Ray ScreenPointToRay(Vector3 p) => default; }
    public struct Ray { }
    public struct RaycastHit { public Collider collider; }
    public static class Physics { public static bool Raycast(Ray ray, out RaycastHit hit) { hit = default; return false; } }
    public struct Quaternion { public static Quaternion identity; }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
        public static float Distance(Vector3 a, Vector3 b) => 0;
    }
    public struct Color
    {
        public Color(float r, float g, float b, float a) { }
        public static Color white, red;
    }
    public struct Rect { public Rect(float x, float y, float w, float h) { } }
    public static class Mathf
    {
        public static float Max(float a, float b) => a;
        public static int Max(int a, int b) => a;
        public static int Min(int a, int b) => a;
        public static int Abs(int v) => v;
        public static float Clamp01(float v) => v;
        public static int RoundToInt(float v) => 0;
    }
    public static class Time { public static float deltaTime, unscaledTime; }
    public static class Screen { public static int width, height; }
    public enum KeyCode { R }
    public static class Input
    {
        public static Vector3 mousePosition;
        public static bool GetMouseButtonDown(int b) => false;
        public static bool GetKeyDown(KeyCode k) => false;
    }
    public static class Debug { public static void Log(object m) { } }
    public enum FontStyle { Bold }
    public enum TextAnchor { MiddleCenter }
    public class GUIStyleState { public Color textColor; }
    public class GUIStyle { public int fontSize; public FontStyle fontStyle; public TextAnchor alignment; public GUIStyleState normal = new GUIStyleState(); }
    public static class GUI
    {
        public static Color color;
        public static void DrawTexture(Rect r, Texture2D t) { }
        public static void Label(Rect r, string text, GUIStyle style) { }
    }
    public class HeaderAttribute : System.Attribute { public HeaderAttribute(string h) { } }
    public class TooltipAttribute : System.Attribute { public TooltipAttribute(string t) { } }
    public class CreateAssetMenuAttribute : System.Attribute { public string fileName, menuName; }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public int buildIndex; }
    public enum LoadSceneMode { Single }
    public static class SceneManager
    {
        public static event System.Action<Scene, LoadSceneMode> sceneLoaded;
        public static Scene GetActiveScene() => default;
        public static void LoadScene(int index) { }
    }
}

namespace TMPro
{
    public class TextMeshProUGUI : UnityEngine.Component { public string text; }
}
