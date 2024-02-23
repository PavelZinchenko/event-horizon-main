using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Features;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Controls;
using Combat.Component.Engine;
using Combat.Component.Physics;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Unit;
using Combat.Component.View;
using Combat.Component.Platform;
using Combat.Component.Ship.Effects;
using Combat.Component.Unit.Classification;
using Combat.Component.Triggers;
using Combat.Unit;
using Constructor;

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
            body.SetVelocityLimit(50f); // TODO
            AddResource(_systems = new ShipSystems(this));
            AddResource(_effects = new ShipEffects(this));
            AddResource(CollisionBehaviour = new DefaultCollisionBehaviour(spec.Stats.RammingDamageMultiplier));
            AddResource(Features = new Features.Features(type.Class == UnitClass.Drone ? TargetPriority.Low : TargetPriority.Normal, spec.Stats.ShipColor.Color));
            AddResource(Stats = stats);
            _state = UnitState.Active;
            Specification = spec;
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
            Affect(impact);
            InvokeTriggers(ConditionType.OnHit);
        }

        public void Affect(Impact impact)
        {
            impact.ApplyImpulse(Body);
            Stats.ApplyDamage(impact);

            Systems.OnEvent(SystemEventType.DamageTaken);
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Stats.Energy.Value > 0)
            {
                Engine.Course = Controls.Course;
                Engine.Throttle = Controls.Throttle;
                Engine.Update(elapsedTime, Body);
            }
            else
            {
                Engine.Throttle = 0;
            }

            Features.UpdatePhysics(elapsedTime, Collider);
            UpdateSystems(elapsedTime);
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
        private readonly ShipSystems _systems;
        private readonly ShipEffects _effects;
    }
}
