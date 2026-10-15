using UnityEngine;

public static class VectorExtensions
{
    public static Vector2 Direction(this Vector2 source, Vector2 target)
    {
        return target - source;
    }

    public static float Distance(this Vector2 source, Vector2 target)
    {
        return Vector2.Distance(source, target);
    }

    public static float SqrDistance(this Vector2 source, Vector2 target)
    {
        return Vector2.SqrMagnitude(source - target);
    }

    public static Vector2 Lerp(this Vector2 from, Vector2 to, float delta, float minStep)
    {
        if (delta <= 0) return from;
        if (delta >= 1) return to;

        var dx = to.x - from.x;
        var dy = to.y - from.y;
        var sqrDistance = dx*dx + dy*dy;
        if (sqrDistance <= minStep*minStep) return to;

        var minDelta = minStep / Mathf.Sqrt(sqrDistance);
        if (delta < minDelta) delta = minDelta;
        return new Vector2(from.x + dx * delta, from.y + dy * delta);
    }
}
