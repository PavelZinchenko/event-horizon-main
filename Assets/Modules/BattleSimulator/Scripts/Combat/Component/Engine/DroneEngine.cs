using Combat.Component.Body;
using Combat.Component.Mods;
using Constructor.Model;
using UnityEngine;

namespace Combat.Component.Engine
{
    public class DroneEngine : IEngine
    {
        public DroneEngine(EngineStats engineStats)
        {
            _engineStats = engineStats;
            UpdateData();
        }

        public float MaxVelocity { get { return _engineData.Velocity; } }
        public float MaxAngularVelocity { get { return _engineData.AngularVelocity; } }
        public float Propulsion { get { return _engineData.Propulsion; } }
        public float TurnRate { get { return _engineData.TurnRate; } }

		public float ForwardAcceleration { get; private set; }

		public float? Course
        {
            get
            {
                if (_engineData.HasCourse)
                    return _engineData.Course;
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _engineData.HasCourse = true;
                    _engineData.Course = value.Value;
                }
                else
                {
                    _engineData.HasCourse = false;
                }
            }
        }

        public float Throttle { get { return _engineData.Throttle; } set { _engineData.Throttle = value; } }

        public Modifications<EngineData> Modifications { get { return _modifications; } }

        public void Update(float elapsedTime, IBody body, bool hasEnergy)
        {
            if (!hasEnergy)
            {
                Throttle = 0;
                return;
            }

            UpdateData();

			ForwardAcceleration = Throttle > 0.01f ? ApplyAcceleration(body, elapsedTime) : 0f;

            if (_engineData.Deceleration > 0)
                ApplyDeceleration(body, elapsedTime);

            if (_engineData.HasCourse)
                ApplyAngularAcceleration(body, elapsedTime);
            else if (Mathf.Abs(body.AngularVelocity) > 0.01f)
                ApplyAngularDeceleration(body, elapsedTime);
        }

        private void UpdateData()
        {
            _engineData.AngularVelocity = _engineStats.AngularVelocity;
            _engineData.Velocity = _engineStats.Velocity;
            _engineData.TurnRate = _engineStats.TurnRate;
            _engineData.Propulsion = _engineStats.Propulsion;
            _engineData.Deceleration = 0;

            _modifications.Apply(ref _engineData);

            if (_engineData.Velocity > _engineStats.VelocityLimit)
                _engineData.Velocity = _engineStats.VelocityLimit;
            if (_engineData.AngularVelocity > _engineStats.AngularVelocityLimit)
                _engineData.AngularVelocity = _engineStats.AngularVelocityLimit;
        }

        private float ApplyAcceleration(IBody body, float elapsedTime)
        {
            var acceleration = Throttle * Propulsion;
            var forward = RotationHelpers.Direction(body.Rotation);
            var velocity = body.Velocity;
            var forwardVelocity = Vector2.Dot(velocity, forward);

			if (forwardVelocity >= MaxVelocity)
				return 0f;

            var requiredVelocity = Mathf.Max(forwardVelocity, MaxVelocity) * forward;
            var direction = (requiredVelocity - velocity).normalized;

            body.ApplyAcceleration(acceleration * elapsedTime * direction);
			return acceleration;
        }

        private void ApplyDeceleration(IBody body, float elapsedTime)
        {
            var velocity = body.Velocity;
            if (velocity.magnitude < 0.001f)
                return;

            var direction = velocity.normalized;
            body.ApplyAcceleration(-_engineData.Deceleration * elapsedTime * direction);
        }

        private void ApplyAngularAcceleration(IBody body, float elapsedTime)
        {
            var angularVelocity = body.AngularVelocity;
            var acceleration = 0f;

            var minDeltaAngle = Mathf.DeltaAngle(body.Rotation, _engineData.Course);

            var deltaAngle = 0f;
            if (minDeltaAngle > 0 && angularVelocity < 0)
                deltaAngle = 360 - minDeltaAngle;
            else if (minDeltaAngle < 0 && angularVelocity > 0)
                deltaAngle = 360 + minDeltaAngle;
            else
                deltaAngle = Mathf.Abs(minDeltaAngle);

            if (deltaAngle < 120f && deltaAngle < angularVelocity * angularVelocity / TurnRate)
                acceleration = Mathf.Clamp(-angularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            else if (minDeltaAngle < 0 && angularVelocity > -MaxAngularVelocity || minDeltaAngle > 0 && angularVelocity < MaxAngularVelocity)
                acceleration = Mathf.Clamp(minDeltaAngle, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            else
                return;

            body.ApplyAngularAcceleration(acceleration);
        }

        private void ApplyAngularDeceleration(IBody body, float elapsedTime)
        {
            var acceleration = Mathf.Clamp(-body.AngularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            body.ApplyAngularAcceleration(acceleration);
        }

        private EngineData _engineData;

        private readonly EngineStats _engineStats;
        private readonly Modifications<EngineData> _modifications = new Modifications<EngineData>();
    }
}
