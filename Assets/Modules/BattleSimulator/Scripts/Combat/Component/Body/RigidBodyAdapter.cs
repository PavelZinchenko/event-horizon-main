﻿using UnityEngine;

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
                return _cachedPosition; 
            }
            set
            {
                _cachedPosition = value;
                if (this && transform)
                    gameObject.Move(value);
            }
        }

        public float Rotation
        {
			get
            {
                return _cachedRotation;
            }
            set
            {
                _cachedRotation = value;
                if (this && transform)
                    transform.localEulerAngles = new Vector3(0, 0,Mathf.Repeat(value, 360));
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
            
            _cachedWorldPosition = _rigidbody.position;
            _cachedWorldRotation = _rigidbody.rotation;
            if (_parent != null)
            {
                _cachedPosition = _parent.WorldPositionToLocal(_cachedWorldPosition);
                _cachedRotation = _parent.WorldRotationToLocal(_cachedWorldRotation);
            }
            else
            {
                _cachedPosition = _cachedWorldPosition;
                _cachedRotation = _cachedWorldRotation;
            }
            
            _cachedVelocity = velocity;
            _cachedAngularVelocity = _rigidbody.angularVelocity;
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
        }

        public Vector2 WorldPosition()
        {
            return _cachedWorldPosition +
                   RotationHelpers.Direction(_cachedWorldRotation) * (Offset * (_parent?.WorldScale() ?? 1));
        }

        public Vector2 WorldPositionNoOffset()
        {
            return _cachedWorldPosition;
        }

        public float WorldRotation()
        {
            return _cachedWorldRotation;
        }
        
        private float _weigth;
        private Vector2 _viewPosition;
        private float _viewRotation;
        private Vector2 _cachedPosition;
        private float _cachedRotation;
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
