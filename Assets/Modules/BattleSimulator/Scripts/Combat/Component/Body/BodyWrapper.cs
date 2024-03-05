using Combat.Component.Body;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class BodyWrapper : IBody
    {
        public BodyWrapper(IBody body)
        {
            _body = body;
        }

        public void Dispose() { }
        public IBody Parent => _body.Parent;
        public Vector2 Position => _body.Position;
        public float Rotation => _body.Rotation + _rotation;
        public float Offset => _body.Offset;
        public Vector2 Velocity => _body.Velocity;
        public float AngularVelocity => _body.AngularVelocity;
        public float Weight => _body.Weight;
        public float Scale => _body.Scale;
        public Vector2 VisualPosition => _body.VisualPosition;
        public float VisualRotation => _body.VisualRotation;

        public void Move(Vector2 position) { }
        public void Turn(float rotation) { _rotation = rotation; }
        public void SetSize(float size) { }
        public void ApplyAcceleration(Vector2 acceleration) { }
        public void ApplyAngularAcceleration(float acceleration) { }
        public void ApplyForce(Vector2 position, Vector2 force) { }
        public void SetVelocityLimit(float value) { }

        public void UpdatePhysics(float elapsedTime) { }
        public void UpdateView(float elapsedTime) { }

        public void AddChild(Transform child)
        {
            _body.AddChild(child);
        }

        public Transform FindChild(string childName)
        {
            return _body.FindChild(childName);
        }

        private float _rotation;
        private readonly IBody _body;
    }
}
