using CombatSimulation.Physics;
using UnityEngine;

namespace CombatView
{
    // The poses of one entity at the last two completed ticks, in canonical torus coordinates and radians.
    // A view draws a pose between them, so the 50 Hz simulation looks smooth at any frame rate.
    public struct PoseTrack
    {
        public void Reset(Vec2 position, float angle)
        {
            _previous = _current = position;
            _previousAngle = _currentAngle = angle;
        }

        public void Push(Vec2 position, float angle)
        {
            _previous = _current;
            _previousAngle = _currentAngle;
            _current = position;
            _currentAngle = angle;
        }

        // Alpha 0 is the previous tick, 1 the current one. Both paths take the short way: across the wrap
        // and around the circle. The position can leave the canonical range; Placement wraps it again.
        public Vec2 SamplePosition(float alpha, Torus torus) => _previous + alpha * torus.Delta(_previous, _current);
        public float SampleAngle(float alpha) => _previousAngle + alpha * DeltaAngle(_previousAngle, _currentAngle);

        private static float DeltaAngle(float from, float to) => Mathf.Repeat(to - from + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;

        private Vec2 _previous;
        private Vec2 _current;
        private float _previousAngle;
        private float _currentAngle;
    }

    public static class Placement
    {
        // The image of a canonical point nearest to the reference point. The reference can lie outside the
        // canonical range (the camera follows a ship across the wrap), so both are wrapped before the delta.
        public static Vector2 NearestImage(Torus torus, Vector2 reference, Vec2 point)
        {
            var origin = torus.Wrap(new Vec2(reference.x, reference.y));
            var delta = torus.Delta(origin, torus.Wrap(point));
            return new Vector2(reference.x + delta.X, reference.y + delta.Y);
        }
    }
}
