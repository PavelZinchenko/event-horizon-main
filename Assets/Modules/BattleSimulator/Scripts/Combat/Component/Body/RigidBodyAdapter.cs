using UnityEngine;

namespace Combat.Component.Body
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyAdapter : MonoBehaviour, IBodyComponent
    {
        public void Initialize(IBody parent, Vector2 position, float rotation, float scale, Vector2 velocity, float angularVelocity, float weight)
        {
            if (parent != null)
                parent.AddChild(transform);
            else
                transform.parent = null;

            Parent = parent;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Weight = weight;

            if (_rigidbody.bodyType != RigidbodyType2D.Static)
            {
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }

            // Required to properly calculate all cached variables
            UpdatePhysics(0);
        }
        
        public IBody Parent
        {
            get { return _parent; }
            private set
            {
                if (_parent == value)
                    return;

                GetComponent<Rigidbody2D>().isKinematic = value != null;
                _parent = value;
            }
        }

        public Vector2 VisualPosition => _viewPosition;
        public float VisualRotation => _viewRotation;

        public Vector2 Position
        {
			get 
            {
                UpdateCachedPosition();
                return _cachedPosition;
            }
            set
            {
                SetCachedWorldPosition(value);
                _positionUpdateTime = Time.fixedTime;
                if (this && transform)
                    gameObject.Move(value);
            }
        }

        public float Rotation
        {
			get 
            {
                UpdateCachedRotation();
                return _cachedRotation;
            }
            set
            {
                SetCachedWorldRotation(value);
                _rotationUpdateTime = Time.fixedTime;
                if (this && transform)
                    transform.localEulerAngles = new Vector3(0, 0, Mathf.Repeat(value, 360));
            }
        }
        
        public float Offset { get; set; }

        public Vector2 Velocity
        {
            get { return Parent == null ? _cachedVelocity : Vector2.zero; }
            set
            {
                if (Parent == null)
                {
                    _cachedVelocity = value;
                    _rigidbody.velocity = value;
                }
            }
        }

        public float AngularVelocity
        {
            get { return Parent == null ? _cachedAngularVelocity : 0f; }
            set
            {
                if (Parent == null && _rigidbody)
                {
                    _cachedAngularVelocity = value;
                    _rigidbody.angularVelocity = value;
                }
            }
        }

        public float Weight
        {
            get { return _weigth; }
            set { _rigidbody.mass = _weigth = value; }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (transform)
                    transform.localScale = Vector3.one * value;
            }
        }

        public void ApplyAcceleration(Vector2 acceleration)
        {
            if (Parent == null)
                _rigidbody.AddForce(acceleration * _rigidbody.mass, ForceMode2D.Impulse);
        }

        public void ApplyAngularAcceleration(float acceleration)
        {
            if (Parent == null)
                _rigidbody.AddTorque(acceleration * Mathf.Deg2Rad * _rigidbody.inertia, ForceMode2D.Impulse);
        }

        public void ApplyForce(Vector2 position, Vector2 force)
        {
            if (Parent == null)
                _rigidbody.AddForceAtPosition(force, position, ForceMode2D.Impulse);
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
            var velocity = _rigidbody.velocity;
            var speed = velocity.magnitude;
            if (speed > _velocityHardLimit)
            {
                velocity = velocity * _velocityHardLimit / speed;
                _rigidbody.velocity = velocity;
            }

            UpdateCachedPosition(true);
            UpdateCachedRotation(true);

            _cachedVelocity = velocity;
            _cachedAngularVelocity = _rigidbody.angularVelocity;
        }

        private void UpdateCachedPosition(bool noThreadCheck = false)
        {
            if (!noThreadCheck && System.Threading.Thread.CurrentThread != _mainThread) return;
            var time = Time.fixedTime;
            if (_positionUpdateTime == time) return;
            SetCachedWorldPosition(_rigidbody.position);
            _positionUpdateTime = time;
        }

        private void SetCachedWorldPosition(in Vector2 position)
        {
            _cachedWorldPosition = position;
            _cachedPosition = _parent == null ? _cachedWorldPosition : _parent.WorldPositionToLocal(_cachedWorldPosition);
        }

        private void UpdateCachedRotation(bool noThreadCheck = false)
        {
            if (!noThreadCheck && System.Threading.Thread.CurrentThread != _mainThread) return;
            var time = Time.fixedTime;
            if (_rotationUpdateTime == time) return;
            SetCachedWorldRotation(_rigidbody.rotation);
            _rotationUpdateTime = time;
        }

        private void SetCachedWorldRotation(float rotation)
        {
            _cachedWorldRotation = rotation;
            _cachedRotation = _parent == null ? _cachedWorldRotation : _parent.WorldRotationToLocal(_cachedWorldRotation);
        }

        public void UpdateView(float elapsedTime)
        {
            var t = transform;
            _viewPosition = t.localPosition;
            _viewRotation = t.localEulerAngles.z;
        }

        public void AddChild(Transform child)
        {
            child.parent = transform;
        }
        
        public Transform FindChild(string childName)
        {
            return transform.Find(childName);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _mainThread = System.Threading.Thread.CurrentThread;
        }

        public Vector2 WorldPosition()
        {
            UpdateCachedPosition();
            if (Offset == 0) return _cachedWorldPosition;
            return _cachedWorldPosition +
                   RotationHelpers.Direction(_cachedWorldRotation) * (Offset * (_parent?.WorldScale() ?? 1));
        }

        public Vector2 WorldPositionNoOffset()
        {
            UpdateCachedPosition();
            return _cachedWorldPosition;
        }

        public float WorldRotation()
        {
            UpdateCachedRotation();
            return _cachedWorldRotation;
        }

        private float _positionUpdateTime;
        private float _rotationUpdateTime;
        private Vector2 _cachedPosition;
        private float _cachedRotation;

        private float _weigth;
        private System.Threading.Thread _mainThread;
        private Vector2 _viewPosition;
        private float _viewRotation;
        private Vector2 _cachedWorldPosition;
        private float _cachedWorldRotation;
        private Rigidbody2D _rigidbody;
        private Vector2 _cachedVelocity;
        private float _cachedAngularVelocity;
        private float _scale;
        private IBody _parent;

        private const float _velocityHardLimit = 120;
    }
}
