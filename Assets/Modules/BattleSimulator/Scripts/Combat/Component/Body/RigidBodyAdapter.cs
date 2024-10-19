using UnityEngine;

namespace Combat.Component.Body
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyAdapter : MonoBehaviour, IBodyComponent
    {
        const float _velocityHardLimit = 120;

        [SerializeField] private bool _limitMaxVelocity = false;

        public void Initialize(IBody parent, Vector2 position, float rotation, float scale, Vector2 velocity, float angularVelocity, float weight)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _mainThread = System.Threading.Thread.CurrentThread;
            _parent = parent;

            if (_parent != null)
            {
                _parent.AddChild(transform);
                _rigidbody.isKinematic = true;
            }
            else
            {
                transform.parent = null;
                _rigidbody.isKinematic = false;
            }

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

        public IBody Parent => _parent;
        public Vector2 VisualPosition => _viewPosition;
        public float VisualRotation => _viewRotation;

        public Vector2 Position
        {
			get 
            {
                UpdatePosition();
                return _position;
            }
            set
            {
                _position = value;
                _positionUpdateTime = Time.fixedTime;

                if (_parent == null)
                {
                    _cachedWorldPosition = _position;
                }
                else
                {
                    _cachedWorldPosition = _parent.WorldPosition() + 
                        RotationHelpers.Transform(_position, _parent.WorldRotation()) * _parent.WorldScale();
                }

                if (this && transform)
                    gameObject.Move(_position);
            }
        }

        public float Rotation
        {
			get 
            {
                UpdateRotation();
                return _rotation;
            }
            set
            {
                _rotation = value;
                _rotationUpdateTime = Time.fixedTime;

                if (_parent == null)
                    _cachedWorldRotation = _rotation;
                else
                    _cachedWorldRotation = _rotation + _parent.WorldRotation();

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

            if (_limitMaxVelocity)
            {
                var speed = velocity.magnitude;
                if (speed > _velocityHardLimit)
                {
                    velocity = velocity * _velocityHardLimit / speed;
                    _rigidbody.velocity = velocity;
                }
            }

            UpdatePosition(true);
            UpdateRotation(true);

            _cachedVelocity = velocity;
            _cachedAngularVelocity = _rigidbody.angularVelocity;
        }

        private void UpdatePosition(bool noThreadCheck = false)
        {
            if (!noThreadCheck && System.Threading.Thread.CurrentThread != _mainThread) return;
            var time = Time.fixedTime;
            if (_positionUpdateTime == time) return;
            _cachedWorldPosition = _rigidbody.position;
            _position = _parent == null ? _cachedWorldPosition : _parent.WorldPositionToLocal(_cachedWorldPosition);
            _positionUpdateTime = time;
        }

        private void UpdateRotation(bool noThreadCheck = false)
        {
            if (!noThreadCheck && System.Threading.Thread.CurrentThread != _mainThread) return;
            var time = Time.fixedTime;
            if (_rotationUpdateTime == time) return;
            _cachedWorldRotation = _rigidbody.rotation;
            _rotation = _parent == null ? _cachedWorldRotation : _parent.WorldRotationToLocal(_cachedWorldRotation);
            _rotationUpdateTime = time;
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

        public Vector2 WorldPosition()
        {
            UpdatePosition();

            if (_cachedWorldPosition == Vector2.zero)
            {
                UnityEngine.Debug.LogError("zero");
            }

            if (Offset == 0) return _cachedWorldPosition;
            return _cachedWorldPosition +
                   RotationHelpers.Direction(_cachedWorldRotation) * (Offset * (_parent?.WorldScale() ?? 1));
        }

        public Vector2 WorldPositionNoOffset()
        {
            UpdatePosition();
            return _cachedWorldPosition;
        }

        public float WorldRotation()
        {
            UpdateRotation();
            return _cachedWorldRotation;
        }

        private float _positionUpdateTime;
        private float _rotationUpdateTime;
        private Vector2 _position;
        private float _rotation;

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
    }
}
