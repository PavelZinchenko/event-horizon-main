using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public sealed class RadarInterferenceAction : ICollisionAction
    {
        public RadarInterferenceAction(float duration, float initialEnergyDrainFraction, float energyDrainPerSecond,
            BulletImpactType impactType)
        {
            _duration = duration;
            _initialEnergyDrainFraction = initialEnergyDrainFraction;
            _energyDrainPerSecond = energyDrainPerSecond;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!collisionData.IsNew || !_isAlive || target is not IShip ship)
                return;

            targetImpact.EnergyDrain += ship.Stats.Energy.MaxValue * _initialEnergyDrainFraction;
            RadarStatus.ApplyJammed(ship, _duration, _energyDrainPerSecond);
            _isAlive = _impactType == BulletImpactType.HitAllTargets;
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _duration;
        private readonly float _initialEnergyDrainFraction;
        private readonly float _energyDrainPerSecond;
        private readonly BulletImpactType _impactType;
    }
}
