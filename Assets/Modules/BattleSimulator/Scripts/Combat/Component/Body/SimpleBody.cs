using UnityEngine;

namespace Combat.Component.Body
{
    public class SimpleBody : MonoBehaviour, IBody
    {
        private float _worldPositionUpdateTime;
        private float _fixedTime = -1f;
        private Vector2 _cachedWorldPosition;
        private Vector2 _position;
        private float _rotation;
        private float _offset;
        private IBody _parent;

        public static SimpleBody Create(IBody parent, Vector2 position, float rotation, float scale, float weight, float offset)
        {
            var gameobject = new GameObject("Body");
            var transform = gameobject.transform;
            if (parent != null)
                parent.AddChild(transform);

            var body = gameobject.AddComponent<SimpleBody>();
            body.Initialize(parent, position, rotation, scale, weight, offset);
            return body;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }

        public IBody Parent 
        {
            get => _parent;
            private set
            {
                _parent = value;
                _worldPositionUpdateTime = 0;
            }
        }

        public Vector2 Position => _position;
        public float Rotation => _rotation;
        public float Offset => _offset;
        public Vector2 Velocity => Vector2.zero;
        public float AngularVelocity => 0f;
        public float Weight { get; private set; }
        public float Scale { get; private set; }
        public Vector2 VisualPosition => Position;
        public float VisualRotation => Rotation;

        public void Move(Vector2 position)
        {
            _position = position;
            transform.localPosition = Parent == null ? Position : Parent.ChildPosition(Position);
            _worldPositionUpdateTime = 0;
        }

        public void Turn(float rotation)
        {
            _rotation = Mathf.Repeat(rotation, 360);
            transform.localEulerAngles = new Vector3(0, 0, Rotation);
            _worldPositionUpdateTime = 0;
        }

        public void SetSize(float size)
        {
            Scale = size;
            transform.localScale = Scale * Vector3.one;
            _worldPositionUpdateTime = 0;
        }

        public void ApplyAcceleration(Vector2 acceleration)
        {
            //if (_parent != null)
            //    _parent.ApplyAcceleration(acceleration);
        }

        public void ApplyAngularAcceleration(float acceleration)
        {
            //if (_parent != null)
            //    _parent.ApplyAngularAcceleration(acceleration);
        }

        public void ApplyForce(Vector2 position, Vector2 force)
        {
            if (Parent != null)
                Parent.ApplyForce(position, force);
        }

        public void SetVelocityLimit(float value) {}

        public void UpdatePhysics(float elapsedTime) 
        {
            _fixedTime = Time.fixedTime;
        }

        public void UpdateView(float elapsedTime) {}

        public void AddChild(Transform child)
        {
            child.parent = transform;
        }
        
        public Transform FindChild(string childName)
        {
            return transform.Find(childName);
        }

        private void Initialize(IBody parent, Vector2 position, float rotation, float scale, float weight, float offset)
        {
            Parent = parent;
            Weight = weight;
            _offset = offset;
            Move(position);
            Turn(rotation);
            SetSize(scale);
        }

        public Vector2 WorldPosition()
        {
            if (_worldPositionUpdateTime != _fixedTime)
            {
                var position = _offset == 0 ? _position : _position + RotationHelpers.Direction(_rotation) * _offset;
                _cachedWorldPosition = Parent.WorldPosition() + RotationHelpers.Transform(position, Parent.WorldRotation()) * Parent.WorldScale();
                _worldPositionUpdateTime = _fixedTime;
            }

            return _cachedWorldPosition;
        }
    }
}
