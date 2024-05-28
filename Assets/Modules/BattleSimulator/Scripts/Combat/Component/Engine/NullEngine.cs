using Combat.Component.Body;
using Combat.Component.Mods;

namespace Combat.Component.Engine
{
    public class NullEngine : IEngine
    {
		public float MaxVelocity => 0;
		public float MaxAngularVelocity => 0;
		public float Propulsion => 0;
		public float TurnRate => 0;

		public float? Course { get => null; set { } }
        public float Throttle { get => 0; set { } }
		public float ForwardAcceleration => 0f;

		public Modifications<EngineData> Modifications => _modifications;

		public void Update(float elapsedTime, IBody body, bool hasEnergy)
        {
            ApplyDeceleration(body, elapsedTime);
            ApplyAngularDeceleration(body, elapsedTime);
        }

        private void ApplyDeceleration(IBody body, float elapsedTime)
        {
            var velocity = body.Velocity;
            if (velocity.sqrMagnitude < 0.001f)
                return;

            body.ApplyAcceleration(-velocity * elapsedTime);
        }

        private void ApplyAngularDeceleration(IBody body, float elapsedTime)
        {
            var angularVelocity = body.AngularVelocity;
            if (angularVelocity < 0.001f && angularVelocity > -0.001f)
                return;

            body.ApplyAngularAcceleration(-angularVelocity * elapsedTime);
        }

        private readonly Modifications<EngineData> _modifications = new Modifications<EngineData>();
    }
}
