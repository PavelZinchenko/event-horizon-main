using System;
using System.Collections.Generic;
using Combat.Component.Physics.Joint;
using UnityEngine;
using UnityEngine.Events;

namespace Combat.Component.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsManager : MonoBehaviour, IDisposable
    {
        [SerializeField] private UnityEvent<DistanceJoint2D> _distanceJointCreated;
        [SerializeField] private UnityEvent<FixedJoint2D> _fixedJointCreated;

        public IJoint CreateDistanceJoint(PhysicsManager other, float maxDistance)
        {
            if (other == null)
                return null;

            var joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = other.Rigidbody;
            joint.autoConfigureDistance = false;
            joint.maxDistanceOnly = true;
            joint.enableCollision = true;
            joint.distance = maxDistance;

            _joints.Add(joint);
            _distanceJointCreated?.Invoke(joint);
            return new Joint<DistanceJoint2D>(joint);
        }

        public Joint.HingeJoint CreateHingeJoint(PhysicsManager other, float offset, float connectedOffset, float minAngle, float maxAngle)
        {
            if (other == null)
                return null;

            var joint = gameObject.AddComponent<HingeJoint2D>();
            joint.connectedBody = other.Rigidbody;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = new Vector2(-offset, 0f);
            joint.connectedAnchor = new Vector2(connectedOffset,0f);
            joint.useLimits = true;
            joint.limits = new JointAngleLimits2D {min = minAngle, max = maxAngle};

            _joints.Add(joint);
            return new Joint.HingeJoint(joint);
        }

        public IJoint CreateFixedJoint(PhysicsManager other, bool enableCollision)
        {
            if (other == null)
                return null;

            var joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = other.Rigidbody;
            joint.autoConfigureConnectedAnchor = false;

            var position = Rigidbody.position;
            var size = Rigidbody.transform.localScale.z;
            var targetPosition = other.Rigidbody.position;
            var targetRotation = other.Rigidbody.rotation;
            var targetSize = other.Rigidbody.transform.localScale.z;

            var offset = position - targetPosition;
            var distance = offset.magnitude;
            if (distance <= size)
            {
                offset = Vector2.zero;
            }
            else
            {
                offset *= (distance - size) / distance;
                offset = RotationHelpers.Transform(offset, -targetRotation) / targetSize;
            }

            joint.anchor = Vector2.zero;
            joint.connectedAnchor = offset;
            joint.enableCollision = enableCollision;

            _joints.Add(joint);
            _fixedJointCreated?.Invoke(joint);
            return new Joint<FixedJoint2D>(joint);
        }

        public void Dispose()
        {
            foreach (var item in _joints)
                GameObject.Destroy(item);
            _joints.Clear();
        }

        protected Rigidbody2D Rigidbody { get { return _rigidbody ?? (_rigidbody = GetComponent<Rigidbody2D>()); } }

        protected Rigidbody2D _rigidbody;
        private readonly List<Joint2D> _joints = new();
    }
}
