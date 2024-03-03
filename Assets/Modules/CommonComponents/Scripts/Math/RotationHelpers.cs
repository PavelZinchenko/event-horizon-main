using UnityEngine;

public static class RotationHelpers
{
	public static Vector2 Direction(float angle)
	{
		return new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad), Mathf.Sin(angle*Mathf.Deg2Rad));
	}

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

	public static Vector2 Transform(Vector2 point, float angle)
	{
		var angleRadians = angle*Mathf.Deg2Rad;
		return new Vector2(
				point.x * Mathf.Cos(angleRadians) - point.y * Mathf.Sin(angleRadians),
				point.y * Mathf.Cos(angleRadians) + point.x * Mathf.Sin(angleRadians));
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
