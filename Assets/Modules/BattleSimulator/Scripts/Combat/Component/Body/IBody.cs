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

        /// <summary>
        /// Moves the body to a new position, in local space
        ///
        /// If body has no parent, position is in world coordinates
        /// </summary>
        /// <param name="position">New position of the body</param>
        void Move(Vector2 position);
        
        /// <summary>
        /// Changes the rotation of the body, in local space
        ///
        /// If body has no parent, rotation is in world space
        /// </summary>
        /// <param name="rotation">New rotation of the body</param>
        void Turn(float rotation);
        
        /// <summary>
        /// Changes the size of the body, in local space
        ///
        /// If body has no parent, size is in world units
        /// </summary>
        /// <param name="size">New size of the body</param>
        void SetSize(float size);
        void ApplyAcceleration(Vector2 acceleration);
        void ApplyAngularAcceleration(float acceleration);
        void ApplyForce(Vector2 position, Vector2 force);

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);

        void AddChild(Transform child);
        
        /// <summary>
        /// Looks up children transforms to find transform with the matching name
        /// </summary>
        /// <param name="childName">name of this child GameObject</param>
        /// <returns>Transform of the found child, or null otherwise</returns>
        public Transform FindChild(string childName);
        
        public Vector2 WorldPosition()
        {
            var position = Offset == 0 ? Position : Position + RotationHelpers.Direction(Rotation) * Offset;

            if (Parent == null)
                return position;

            return Parent.WorldPosition() +
                   RotationHelpers.Transform(position, Parent.WorldRotation()) * Parent.WorldScale();
        }

        public Vector2 VisualWorldPosition()
        {
            var position = Offset == 0 ? VisualPosition : VisualPosition + RotationHelpers.Direction(VisualRotation) * Offset;

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
