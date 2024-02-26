using UnityEngine;

namespace Combat.Component.Physics.Joint
{
    public class Joint<T> : IJoint where T : Joint2D
    {
        private readonly T _joint;

        public Joint(T joint)
        {
            _joint = joint;
        }

        public bool IsActive => _joint && _joint.connectedBody;

        public void Dispose()
        {
            if (_joint)
                Object.Destroy(_joint);
        }

        protected T Component => _joint;
    }
}
