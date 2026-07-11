using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Features;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Controls;
using Combat.Component.Bullet;
using Combat.Component.Controller;
using Combat.Component.Engine;
using Combat.Component.Physics;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Systems.Devices;
using Combat.Component.Unit;
using Combat.Component.View;
using Combat.Component.Platform;
using Combat.Component.Ship.Effects;
using Combat.Component.Ship.Effects.Special;
using Combat.Component.Unit.Classification;
using Combat.Component.Triggers;
using Combat.Unit;
using Combat.Unit.Object;
using Constructor;
using UnityEngine;

namespace Combat.Component.Ship
{
    public class Ship : UnitBase, IShip
    {
        public Ship(IShipSpecification spec, IShip owner, IBody body, IView view, IStats stats, ICollider collider, PhysicsManager physics)
            : this(spec, new UnitType(UnitClass.Drone, UnitSide.Undefined, owner), body, view, stats, collider, physics)
        {
        }

        public Ship(IShipSpecification spec, UnitSide side, IBody body, IView view, IStats stats, ICollider collider, PhysicsManager physics)
            : this(spec, new UnitType(UnitClass.Ship, side, null), body, view, stats, collider, physics)
        {
        }

        protected Ship(IShipSpecification spec, UnitType type, IBody body, IView view, IStats stats, ICollider collider, PhysicsManager physics)
            : base(type, body, view, collider, physics)
        {
            AddResource(_systems = new ShipSystems(this));
            AddResource(_effects = new ShipEffects(this));
            AddResource(CollisionBehaviour = new DefaultCollisionBehaviour(spec.Stats.RammingDamageMultiplier));
            AddResource(Features = new Features.Features(type.Class == UnitClass.Drone ? TargetPriority.Low : TargetPriority.Normal, spec.Stats.ShipColor.Color));
            AddResource(Stats = stats);
            _state = UnitState.Active;
            Specification = spec;
            if (spec.Info.Id.Value == 166)
                _effects.TryAdd(new WaterdropHaloEffect());
        }

        public override UnitState State => _state;
        public IStats Stats { get; }
        public IShipSpecification Specification { get; }
        public IEngine Engine { get; set; }
        public IControls Controls { get; set; }
        public IFeatures Features { get; }
        public IShipSystems Systems => _systems;
        public IShipEffects Effects => _effects;
		public Combat.Helpers.RadioTransmitter RadioTransmitter { get; set; }

		public void Broadcast(string message, UnityEngine.Color color) => RadioTransmitter?.Broadcast(this, message, color);

		public override float DefenseMultiplier => Stats.HitPointsMultiplier;

        public override ICollisionBehaviour CollisionBehaviour { get; }

        public override void OnCollision(Impact impact, IUnit target, CollisionData collisionData)
        {
            Affect(impact, target);
            InvokeTriggers(ConditionType.OnHit);
        }

        public bool TryHandleWaterdropCollision(IUnit target, CollisionData collisionData)
        {
            if (Specification.Info.Id.Value != 166 || target == null || !target.IsActive())
                return false;

            if (target is Combat.Component.Bullet.Bullet laser && target.Type.Owner != this &&
                laser.IsReflectableByWaterdrop &&
                (laser.Controller is BeamController || laser.Controller is MovingBeamController))
            {
                // Calculate a surface normal from the actual contact point and
                // redirect the intact beam.  Changing Owner before it travels
                // on makes all later damage and kill credit belong to 水滴.
                var normal = (collisionData.Position - Body.WorldPosition()).normalized;
                if (normal.sqrMagnitude < 0.0001f)
                    normal = -target.Body.WorldVelocity().normalized;

                var velocity = target.Body.WorldVelocity();
                var incomingDirection = velocity.sqrMagnitude > 0.001f
                    ? velocity.normalized
                    : target.Type.Owner != null && target.Type.Owner.IsActive()
                        ? (collisionData.Position - target.Type.Owner.Body.WorldPosition()).normalized
                        : -normal;
                var reflectedDirection = Vector2.Reflect(incomingDirection, normal).normalized;
                var reflectedRotation = RotationHelpers.Angle(reflectedDirection);

                // Detach the beam controller from its original weapon and
                // relocate its origin to the reflection point.  The live beam
                // now travels and deals damage as a projectile owned by 水滴.
                laser.Controller?.Dispose();
                laser.Controller = null;
                if (target.Body.Parent != null)
                {
                    target.Body.Move(target.Body.WorldPositionToLocal(collisionData.Position + reflectedDirection * 0.05f));
                    target.Body.Turn(target.Body.WorldRotationToLocal(reflectedRotation));
                }
                else
                {
                    target.Body.Move(collisionData.Position + reflectedDirection * 0.05f);
                    target.Body.Turn(reflectedRotation);
                }
                target.Type.Owner = this;
                target.Type.FactionId = Type.FactionId;
                if (laser.Collider != null)
                {
                    laser.Collider.Unit = laser;
                    laser.Collider.Source = this;
                }
                var reflectedSpeed = Mathf.Max(velocity.magnitude, 180f);
                target.Body.ApplyAcceleration(reflectedDirection * reflectedSpeed - target.Body.Velocity);
                WaterdropHaloEffect.ShowReflection(collisionData.Position, reflectedDirection, Body.WorldScale());
                return true;
            }

            if (target is Combat.Component.Bullet.Bullet projectile)
            {
                // Missiles must follow the normal collision/detonation path so
                // their area thermal damage is created and can damage 水滴.
                if (projectile.Type.Class == UnitClass.Missile)
                    return false;

                // Resolve the projectile as the attacking unit even when the
                // physics callback reports the ship first.  Without this, a
                // single-hit shot damaged 水滴 but never ran its own destroy
                // action and appeared to pass straight through indefinitely.
                var projectileImpact = new Impact();
                var waterdropImpact = new Impact();
                projectile.CollisionBehaviour?.Process(projectile, this, collisionData,
                    ref projectileImpact, ref waterdropImpact);
                projectile.OnCollision(projectileImpact, this, collisionData);
                OnCollision(waterdropImpact, projectile, collisionData);
                if (collisionData.IsNew && projectile.IsActive() &&
                    projectile.Type.Class == UnitClass.EnergyBolt)
                    projectile.Vanish();
                return true;
            }

            if (target is Asteroid)
            {
                var impact = new Impact();
                impact.Effects |= CollisionEffect.Destroy;
                target.OnCollision(impact, this, collisionData);
                return true;
            }

            if (CombatRelations.AreEnemies(Type, target.Type))
            {
                var impact = new Impact();
                impact.AddDamage(GameDatabase.Enums.DamageType.Impact, 10000f);
                target.OnCollision(impact, this, collisionData);

                // Penetration no longer makes 水滴 collision-invulnerable.
                // It receives a compact ramming hit while retaining its
                // straight-through trajectory.
                var selfImpact = new Impact();
                var collisionDamage = Mathf.Max(1f,
                    collisionData.RelativeVelocity.magnitude * Mathf.Max(1f, target.Body.Weight) * 0.1f);
                selfImpact.AddDamage(GameDatabase.Enums.DamageType.Impact, collisionDamage);
                OnCollision(selfImpact, target, collisionData);
                return true;
            }

            return false;
        }

        public void Affect(Impact impact, IUnit source)
        {
            impact.ApplyImpulse(Body);
            Stats.ApplyDamage(impact, this, source);

            Systems.OnEvent(SystemEventType.DamageTaken);
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            var hasEnergy = Stats.Energy.Value > 0;
            Engine.Course = Controls.Course;
            Engine.Throttle = Controls.Throttle;
            Engine.Update(elapsedTime, Body, hasEnergy);

            Features.UpdatePhysics(elapsedTime, Collider);
            UpdateSystems(elapsedTime);
            ApplyVelocityLimit();
        }

        private void ApplyVelocityLimit()
        {
            foreach (var system in Systems.All)
                if (system is WarpDrive warpDrive && warpDrive.IsWarping)
                    return;

            var limit = Type.Side == UnitSide.Player
                ? PlayerPrefs.GetInt(EngineThrottleKey, 0) != 0
                    ? Mathf.Clamp(PlayerPrefs.GetFloat(EngineThrottleLimitKey, 40f), 20f, 120f)
                    : float.PositiveInfinity
                : 40f;
            var velocity = Body.Velocity;
            if (velocity.sqrMagnitude <= limit * limit)
                return;

            var limited = velocity.normalized * limit;
            if (Body is RigidBodyAdapter rigidBody)
                rigidBody.Velocity = limited;
            else
                Body.ApplyAcceleration(limited - velocity);
        }

        protected override void OnUpdateView(float elapsedTime)
        {
            Features.UpdateView(elapsedTime, View);
            Systems.UpdateView(elapsedTime);
            _effects.UpdateView(elapsedTime);
        }

        protected override void OnDispose() 
		{
			_systems.Dispose();
		}

		public override void Vanish()
        {
            _state = UnitState.Inactive;
        }

        public void AddPlatform(IWeaponPlatform platform)
        {
            _systems.Add(platform);
        }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);

            var engineModification = system.EngineModification;
            if (engineModification != null)
                Engine.Modifications.Add(engineModification);

            var appearanceModification = system.FeaturesModification;
            if (appearanceModification != null)
                Features.Modifications.Add(appearanceModification);

            var systemsModification = system.SystemsModification;
            if (systemsModification != null)
                Systems.Modifications.Add(systemsModification);

            var statsModification = system.StatsModification;
            if (statsModification != null)
                Stats.Modifications.Add(statsModification);

            var trigger = system.UnitAction;
            if (trigger != null)
                AddTrigger(trigger);
        }

        public void AddEffect(IShipEffect shipEffect)
        {
            if (!_effects.TryAdd(shipEffect))
                return;

            var engineModification = shipEffect.EngineModification;
            if (engineModification != null)
                Engine.Modifications.Add(engineModification);

            var appearanceModification = shipEffect.FeaturesModification;
            if (appearanceModification != null)
                Features.Modifications.Add(appearanceModification);

            var systemsModification = shipEffect.SystemsModification;
            if (systemsModification != null)
                Systems.Modifications.Add(systemsModification);

            var statsModification = shipEffect.StatsModification;
            if (statsModification != null)
                Stats.Modifications.Add(statsModification);

            var trigger = shipEffect.UnitAction;
            if (trigger != null)
                AddTrigger(trigger);
        }

        private void UpdateSystems(float elapsedTime)
        {
            if (_state != UnitState.Active)
                return;

            Stats.UpdatePhysics(elapsedTime);
            if (!Stats.IsAlive)
            {
                OnDestroy();
                return;
            }

            Systems.UpdatePhysics(elapsedTime);
            _effects.UpdatePhysics(elapsedTime);
        }

        private void OnDestroy()
        {
            InvokeTriggers(ConditionType.OnDestroy);
            _state = UnitState.Destroyed;
        }

        private UnitState _state;
        private const string EngineThrottleKey = "Preview14.EngineThrottle";
        private const string EngineThrottleLimitKey = "Preview18.EngineThrottleLimit";
        private readonly ShipSystems _systems;
        private readonly ShipEffects _effects;
    }
}
