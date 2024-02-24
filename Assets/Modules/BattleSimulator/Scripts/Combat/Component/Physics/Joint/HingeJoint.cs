using UnityEngine;

namespace Combat.Component.Physics.Joint
{
    public class HingeJoint : Joint<HingeJoint2D>
    {
        public HingeJoint(HingeJoint2D joint)
            : base(joint) 
        {
        }

        public float Angle => Component.jointAngle;

        public float Distance
        {
            get
            {
                var body = Component.attachedRigidbody;
                var connectedBody = Component.connectedBody;
                var distance = Vector2.Distance(body.position, connectedBody.position) / connectedBody.transform.localScale.z;
                return distance;
            }
        }
    }
}
