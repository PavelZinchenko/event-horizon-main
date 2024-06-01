using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Physics;
using Combat.Component.Ship;
using Combat.Component.View;

namespace Combat.Component.Unit
{
    public class WormSegmentDestroyable : WormSegmentBase
    {
        private const float _impulseReactionFactor = 0.25f;
        private const float _tearUpDamageThreshold = 5f;

        private readonly float _maxHitPoints;
        private float _hitPoints;
        private float _recentDamage;

        public WormSegmentDestroyable(IShip ship, IBody body, IView view, ICollider collider, PhysicsManager physics, float hitPoints, 
            float initialCooldown, float respawnCooldown, IWormSegmentFactory factory)
            : base(ship, body, view, collider, physics, initialCooldown, respawnCooldown, factory)
        {
            _maxHitPoints = _hitPoints = hitPoints;
        }

        public override void Affect(Impact impact, IUnit source)
        {
            impact.Impulse?.Apply(Body, _impulseReactionFactor);

            var damage = impact.GetTotalDamage(Resistance.Empty);
            _recentDamage += damage;

            if (_recentDamage > _maxHitPoints || impact.Effects.Contains(CollisionEffect.Destroy))
                Destroy();
            else
                TryDealDamage(ref damage);
        }

        public override bool TryDealDamage(ref float damage)
        {
            if (damage <= 0) return true;

            if (SelfDestructPenging)
            {
                _hitPoints -= damage;
                if (_hitPoints <= 0) Destroy();
                return true;
            }

            if (NextSegment != null && NextSegment.TryDealDamage(ref damage))
                return true;

            if (_hitPoints <= 0) return false;
            if (_hitPoints > damage)
            {
                ResetSpawnCooldown();
                _hitPoints -= damage;
                damage = 0;
                return true;
            }

            damage -= _hitPoints;
            Destroy();
            return false;
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            _recentDamage -= elapsedTime * _maxHitPoints * _tearUpDamageThreshold;
            if (_recentDamage < 0) _recentDamage = 0;

            base.OnUpdatePhysics(elapsedTime);
        }

        public class Factory : IUnitFactory<WormSegmentDestroyable>
        {
            private readonly float _hitPoints;
            private readonly float _spawnCooldown;
            private readonly float _respawnTime;
            private readonly IWormSegmentFactory _wormSegmentFactory;

            public Factory(float hitPoints, float spawnCooldown, float respawnTime, IWormSegmentFactory wormSegmentFactory)
            {
                _hitPoints = hitPoints;
                _spawnCooldown = spawnCooldown;
                _respawnTime = respawnTime;
                _wormSegmentFactory = wormSegmentFactory;
            }

            public WormSegmentDestroyable Create(IShip owner, IBody body, IView view, ICollider collider, PhysicsManager physics)
            {
                return new WormSegmentDestroyable(owner, body, view, collider, physics, _hitPoints, _spawnCooldown, _respawnTime, _wormSegmentFactory);
            }
        }
    }
}
