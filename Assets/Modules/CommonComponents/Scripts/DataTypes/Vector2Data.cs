using System;

namespace GameDatabase.Model
{
    // Engine-free replacement for UnityEngine.Vector2. Field names match Unity, so JSON stays the same.
    public struct Vector2Data
    {
        public float x;
        public float y;

        public Vector2Data(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static readonly Vector2Data Zero = new Vector2Data(0f, 0f);

        public float magnitude => (float)Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;

        public static Vector2Data operator +(Vector2Data a, Vector2Data b) => new Vector2Data(a.x + b.x, a.y + b.y);
        public static Vector2Data operator -(Vector2Data a, Vector2Data b) => new Vector2Data(a.x - b.x, a.y - b.y);
        public static Vector2Data operator -(Vector2Data a) => new Vector2Data(-a.x, -a.y);
        public static Vector2Data operator *(Vector2Data a, float d) => new Vector2Data(a.x * d, a.y * d);
        public static Vector2Data operator *(float d, Vector2Data a) => new Vector2Data(a.x * d, a.y * d);
        public static Vector2Data operator /(Vector2Data a, float d) => new Vector2Data(a.x / d, a.y / d);

        public override string ToString() => $"({x}, {y})";
    }
}
