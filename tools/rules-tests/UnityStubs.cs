// The two UnityEngine types the rules use, so the rules compile and run in plain .NET.
// why: Unity is not installed on CI machines; the real types behave the same for what the rules need
// (integer fields, +, ==, and use as a dictionary/set key).
namespace UnityEngine
{
    public struct Vector2Int : System.IEquatable<Vector2Int>
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);
        public bool Equals(Vector2Int other) => this == other;
        public override bool Equals(object obj) => obj is Vector2Int other && this == other;
        public override int GetHashCode() => x * 73856093 ^ y * 19349663;
        public override string ToString() => $"({x}, {y})";
    }
}
