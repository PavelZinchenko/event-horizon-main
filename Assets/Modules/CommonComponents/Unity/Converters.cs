using GameDatabase.Model;
using UnityEngine;

// Converts engine-free value types to Unity types and back.
public static class UnityConverters
{
    public static Vector2 ToUnity(this Vector2Data value) => new Vector2(value.x, value.y);
    public static Vector2Data ToData(this Vector2 value) => new Vector2Data(value.x, value.y);

    public static Color ToUnity(this ColorData value) => new Color(value.R, value.G, value.B, value.A);
    public static ColorData ToData(this Color value) => new ColorData(value.r, value.g, value.b, value.a);
}
