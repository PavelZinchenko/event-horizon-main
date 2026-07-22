using Combat.Collision;
using Combat.Component.Controller;
using Combat.Component.Unit;

namespace Combat.Component.DamageHandler
{
    public sealed class BallLightningDamageHandler : IDamageHandler
    {
        public BallLightningDamageHandler(BallLightningController controller)
        {
            _controller = controller;
        }

        public CollisionEffect ApplyDamage(Impact impact, IUnit source)
        {
            var damage = impact.GetTotalDamage(Resistance.Empty);
            if (damage > 0f || impact.Effects.Contains(CollisionEffect.Destroy))
                _controller.ReceiveDamage(damage);
            return CollisionEffect.None;
        }

        public void Dispose() { }

        private readonly BallLightningController _controller;
    }
}
