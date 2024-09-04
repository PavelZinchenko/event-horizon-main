using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Physics;
using Combat.Component.Ship;
using Combat.Component.View;

namespace Combat.Component.Unit
{
    public class FixedWormSegment : WormSegmentBase
    {
        private const float _impulseReactionFactor = 0.1f;

        private readonly float _regenerationCooldown;
        private readonly float _maxHitPoints;
        private float _hitPoints;
        private float _timeToRegeneration;

        public FixedWormSegment(IShip ship, IBody body, IView view, ICollider collider, PhysicsManager physics, float hitPoints,
            float initialCooldown, float respawnCooldown, IWormSegmentFactory factory)
            : base(ship, body, view, collider, physics, initialCooldown, respawnCooldown, factory)
        {
            _maxHitPoints = _hitPoints = hitPoints;
        }

        public override int Length 
        {
            get
            {
                var length = _hitPoints > 0 ? 1 : 0;
                if (NextSegment != null)
                    length += NextSegment.Length; 

                return length;
            }
        }

        public override void Affect(Impact impact, IUnit source)
        {
            impact.Impulse?.Apply(Body, _impulseReactionFactor);
            var damage = impact.GetTotalDamage(Resistance.Empty);

            if (TryDealDamage(ref damage)) return;

            if (Attached)
            {
                _timeToRegeneration = _regenerationCooldown;
                Type.Owner?.Affect(new Impact { KineticDamage = damage }, source);
            }
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

            if (!Attached) Destroy();
            damage -= _hitPoints;
            return false;
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (_timeToRegeneration > 0)
            {
                _timeToRegeneration -= elapsedTime;
                if (_timeToRegeneration <= 0)
                    _hitPoints = _maxHitPoints;
            }

            base.OnUpdatePhysics(elapsedTime);
        }

        public class Factory : IUnitFactory<FixedWormSegment>
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

            public FixedWormSegment Create(IShip owner, IBody body, IView view, ICollider collider, PhysicsManager physics)
            {
                return new FixedWormSegment(owner, body, view, collider, physics, _hitPoints, _spawnCooldown, _respawnTime, _wormSegmentFactory);
            }
        }
    }
}
