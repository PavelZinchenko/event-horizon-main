using System.Runtime.CompilerServices;
using UnityEngine;

public static class RotationHelpers
{
    private const int _maxAngle = 360;
    private const int _angleToIndexFactor = 5;
    private const int _directionArraySize = _maxAngle*_angleToIndexFactor;
    private static readonly Vector2[] _directions;

    static RotationHelpers()
    {
        _directions = new Vector2[_directionArraySize+1];
        for (int i = 0; i <= _directionArraySize; ++i)
            _directions[i] = new Vector2(Mathf.Cos(i*Mathf.Deg2Rad/_angleToIndexFactor), Mathf.Sin(i*Mathf.Deg2Rad/_angleToIndexFactor));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Vector2 Direction(float angle)
	{
        var index = Mathf.RoundToInt(Mathf.Repeat(angle, _maxAngle) *_angleToIndexFactor);
		return ref _directions[index];
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(Vector2 direction)
	{
		var result = Mathf.Atan2(direction.y, direction.x);
		if (result < 0) result += Mathf.PI*2;
		return result*Mathf.Rad2Deg;
	}

	public static bool Equals(float firstAngle, float secondAngle)
	{
		return Mathf.Approximately(Mathf.DeltaAngle(firstAngle, secondAngle), 0);
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(Vector2 point, float angle)
	{
        var cossin = Direction(angle);
        return new Vector2(
                point.x * cossin.x - point.y * cossin.y,
                point.y * cossin.x + point.x * cossin.y);
    }

    public static Vector2 BoundingRect(float width, float height, float rotation)
    {
        var angleRadians = rotation * Mathf.Deg2Rad;
        float cosAngle = Mathf.Abs(Mathf.Cos(angleRadians));
        float sinAngle = Mathf.Abs(Mathf.Sin(angleRadians));
        float boundWidth = height * sinAngle + width * cosAngle;
        float boundHeight = height * cosAngle + width * sinAngle;
        return new Vector2(boundWidth, boundHeight);
    }

    public static bool IsRotationInArc(float rotation, float minAngle, float maxAngle)
    {
        rotation = NormalizeAngle(rotation);
        minAngle = NormalizeAngle(minAngle);
        maxAngle = NormalizeAngle(maxAngle);

        if (minAngle < maxAngle)
            return rotation >= minAngle && rotation <= maxAngle;
        else
            // The arc crosses 0 degrees
            return rotation >= minAngle || rotation <= maxAngle;
    }

    public static float NormalizeAngle(float angle)
    {
        if (angle >= 0 && angle < 360) return angle;

        angle %= 360;
        return angle < 0 ? angle + 360 : angle;
    }
}
