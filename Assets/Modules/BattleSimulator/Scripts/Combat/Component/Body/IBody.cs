using System;
using UnityEngine;

namespace Combat.Component.Body
{
    public interface IBody : IDisposable
    {
        IBody Parent { get; }

        Vector2 Position { get; }
        float Rotation { get; }
        float Offset { get; }

        Vector2 Velocity { get; }
        float AngularVelocity { get; }

        float Weight { get; }
        float Scale { get; }

        Vector2 VisualPosition { get; }
        float VisualRotation { get; }

        void Move(Vector2 position);
        void Turn(float rotation);
        void SetSize(float size);
        void ApplyAcceleration(Vector2 acceleration);
        void ApplyAngularAcceleration(float acceleration);
        void ApplyForce(Vector2 position, Vector2 force);
        void SetVelocityLimit(float value);

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);

        void AddChild(Transform child);
        
        public Vector2 WorldPosition()
        {
            var position = Position + RotationHelpers.Direction(Rotation)*Offset;

            if (Parent == null)
                return position;

            return Parent.WorldPosition() +
                   RotationHelpers.Transform(position, Parent.WorldRotation()) * Parent.WorldScale();
        }

        public Vector2 VisualWorldPosition()
        {
            var position = VisualPosition + RotationHelpers.Direction(VisualRotation) * Offset;

            if (Parent == null)
                return position;

            return Parent.VisualWorldPosition() + RotationHelpers.Transform(position, Parent.VisualWorldRotation()) * Parent.WorldScale();
        }

        public Vector2 ChildPosition(Vector2 position)
        {
            return new Vector2(Offset/Scale + position.x, position.y);
        }
        
        public Vector2 WorldPositionNoOffset()
        {
            var position = Position;

            if (Parent == null)
                return position;

            return Parent.WorldPosition() +
                   RotationHelpers.Transform(position, Parent.WorldRotation()) * Parent.WorldScale();
        }

        public float WorldRotation()
        {
            if (Parent == null)
                return Rotation;

            return Rotation + Parent.WorldRotation();
        }

        public float VisualWorldRotation()
        {
            if (Parent == null)
                return VisualRotation;

            return VisualRotation + Parent.VisualWorldRotation();
        }

        public Vector2 WorldVelocity()
        {
            if (Parent == null)
                return Velocity;

            return Parent.Velocity + RotationHelpers.Transform(Velocity, Parent.WorldRotation());
        }

        public float WorldAngularVelocity()
        {
            if (Parent == null)
                return AngularVelocity;

            return AngularVelocity + Parent.WorldAngularVelocity();
        }

        public float WorldScale()
        {
            if (Parent == null)
                return Scale;

            return Scale * Parent.WorldScale();
        }

        public float TotalWeight()
        {
            if (Parent == null)
                return Weight;

            return Weight + Parent.TotalWeight();
        }
    }

    public static class BodyExtensions
    {
        /// <summary>
        /// Converts world position to the position inside local coordinate system of the body
        /// </summary>
        /// <param name="body">Body to use as the origin of the coordinate system</param>
        /// <param name="worldPosition">Position in the world</param>
        /// <returns>Position in the body's local coordinates</returns>
        public static Vector2 WorldPositionToLocal(this IBody body, Vector2 worldPosition)
        {
            return RotationHelpers.Transform((worldPosition - body.Parent.WorldPosition()) / body.WorldScale(), -body.WorldRotation());
        }

        /// <summary>
        /// Converts world rotation to the rotation inside local coordinate system of the body
        /// </summary>
        /// <param name="body">Body to use as the origin of the coordinate system</param>
        /// <param name="worldRotation">Rotation in the world</param>
        /// <returns>Rotation in the body's local coordinates</returns>
        public static float WorldRotationToLocal(this IBody body, float worldRotation)
        {
            return worldRotation - body.WorldRotation();
        }

        public static IBody Owner(this IBody body)
        {
            var parent = body;
            while (parent.Parent != null)
                parent = parent.Parent;

            return parent;
        }
    }
}
