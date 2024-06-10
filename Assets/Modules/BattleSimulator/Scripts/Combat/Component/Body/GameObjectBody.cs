using UnityEngine;

namespace Combat.Component.Body
{
    public class GameObjectBody : MonoBehaviour, IBodyComponent
    {
		[SerializeField] private bool _interpolation = true;

        public void Initialize(IBody parent, Vector2 position, float rotation, float scale, Vector2 velocity, float angularVelocity, float weight)
        {
            _parent = parent;
            if (parent != null)
                parent.AddChild(transform);
            else
                transform.parent = null;

            Position = position;
            Rotation = rotation;
            Scale = scale;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Weight = weight;
            Offset = 0;
        }

        public IBody Parent => _parent;
        public Vector2 VisualPosition => _viewPosition;
        public float VisualRotation => _viewRotation;

        public Vector2 Position
		{
			get => _position;
            set => SetPosition(value, Time.fixedTime);
		}

		public float Rotation
		{
			get => _rotation;
            set => SetRotation(value, Time.fixedTime);
		}

        public float Offset { get; set; }
        public Vector2 Velocity { get; set; }
        public float AngularVelocity { get; set; }
        public float Weight { get; set; }

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                if (transform)
                    transform.localScale = Vector3.one * value;
            }
        }

        public void ApplyAcceleration(Vector2 acceleration) => Velocity += acceleration;
		public void ApplyAngularAcceleration(float acceleration) => AngularVelocity += acceleration;

        public void ApplyForce(Vector2 position, Vector2 force)
        {
            if (Parent != null)
                Parent.ApplyForce(position, force);
        }

        public void Move(Vector2 position)
        {
            Position = position;
        }

        public void Turn(float rotation)
        {
            Rotation = rotation;
        }

        public void SetSize(float size)
        {
            Scale = size;
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
			if (Parent != null)
                return;

            var time = Time.fixedTime;
            if (time > _positionUpdateTime)
                SetPosition(_position + Velocity * (time - _positionUpdateTime), time);
            if (time > _rotationUpdateTime)
                SetRotation(_rotation + AngularVelocity * (time - _rotationUpdateTime), time);
		}

		public void UpdateView(float elapsedTime) 
		{
			if (!_interpolation) return;
			if (_parent != null) return;
			if (!this || !transform) return;

			var deltaTime = (float)(Time.timeAsDouble - Time.fixedTimeAsDouble);
            _viewRotation = _rotation + AngularVelocity*deltaTime;
            SetTransformRotation(_viewRotation);
			_viewPosition = _position + Velocity*deltaTime;
            SetTransformPosition(_viewPosition);
		}

		public void AddChild(Transform child)
        {
            child.parent = transform;
        }
        
		public Transform FindChild(string childName)
		{
			return transform.Find(childName);
		}

        private void SetPosition(in Vector2 position, float time)
        {
            _position = position;
            _positionUpdateTime = time;
            SetTransformPosition(position);
        }

        private void SetRotation(float rotation, float time)
        {
            _rotation = rotation;
            _rotationUpdateTime = time;
            SetTransformRotation(rotation);
        }

        private void SetTransformPosition(Vector2 position)
        {
            if (this && transform)
                gameObject.Move(_parent == null ? position : _parent.ChildPosition(position));
        }

        private void SetTransformRotation(float rotation)
        {
            if (this && transform)
                transform.localEulerAngles = new Vector3(0, 0, Mathf.Repeat(rotation, 360));
        }

        private float _positionUpdateTime;
        private float _rotationUpdateTime;

        private Vector2 _position;
        private float _rotation;
        private Vector2 _viewPosition;
        private float _viewRotation;
        private float _scale;
        private IBody _parent;
    }
}
