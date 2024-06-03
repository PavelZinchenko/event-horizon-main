using Combat.Ai;
using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Controls;
using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Physics;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit;
using Constructor;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Domain
{
    public class ShipInfo : IShipInfo
    {
        public ShipInfo(Constructor.Ships.IShip shipData, IShipSpecification shipSpec, UnitSide unitSide)
        {
            _unitSide = unitSide;
            _shipData = shipData;
            _shipSpec = shipSpec;
        }

        public ShipStatus Status
        {
            get
            {
                if (ShipUnit == null || ShipUnit.State == UnitState.Inactive)
                    return ShipStatus.Ready;
                if (ShipUnit.State == UnitState.Destroyed)
                    return ShipStatus.Destroyed;
                
                return ShipStatus.Active;
            }
        }

        public IShip ShipUnit { get; private set; }
        public Constructor.Ships.IShip ShipData { get { return _shipData; } }
        public float ActivationTime { get; private set; }

        public float Condition
        {
            get
            {
                if (Status == ShipStatus.Destroyed)
                    return 0;
                if (ShipUnit == null)
                    return 1.0f;

                return ShipUnit.Stats.Armor.Percentage;
            }
        }

        public UnitSide Side { get { return _unitSide; } }

        public void Create(Factory.ShipFactory factory, Vector2 position, int aiLevel)
        {
            if (Status != ShipStatus.Ready)
                return;

            var random = new System.Random();
            var rotation = random.Next(360);

            IShip ship;
            if (_shipData.Model.ShipType == ShipType.Starbase)
                ship = factory.CreateStarbase(_shipSpec, position, rotation, _unitSide);
            else if (_unitSide == UnitSide.Player)
                ship = factory.CreatePlayerShip(_shipSpec, position, rotation);
            else
                ship = factory.CreateEnemyShip(_shipSpec, position, rotation, aiLevel);

            if (ShipUnit != null && ShipUnit.State == UnitState.Inactive)
            {
                ship.Stats.Armor.Get(ShipUnit.Stats.Armor.MaxValue - ShipUnit.Stats.Armor.Value);
                ship.Stats.Shield.Get(ShipUnit.Stats.Shield.MaxValue - ShipUnit.Stats.Shield.Value);
            }

            ShipUnit = ship;
            ActivationTime = Time.time;
        }

        public void Destroy()
        {
            if (ShipUnit.IsActive())
                ShipUnit.Vanish();

            ShipUnit = new DeadShip();
        }

        private readonly UnitSide _unitSide;
        private readonly Constructor.Ships.IShip _shipData;
        private readonly IShipSpecification _shipSpec;

        private class DeadShip : IShip
        {
            public UnitType Type { get { return null; } }
            public IBody Body { get { return null; } }
            public IView View { get { return null; } }
            public ICollider Collider { get { return null; } }
            public ICollisionBehaviour CollisionBehaviour { get { return null; } }
            public float DefenseMultiplier => 1.0f;
            public int SpawnerId => 0;
            public PhysicsManager Physics { get { return null; } }
            public UnitState State { get { return UnitState.Destroyed; } }
            public void OnCollision(Impact impact, IUnit target, CollisionData collisionData) {}
            public void UpdatePhysics(float elapsedTime) {}
            public void UpdateView(float elapsedTime) {}
            public void Vanish() {}
            public IControls Controls { get { return null; } }
            public IStats Stats { get { return null; } }
            public IEngine Engine { get { return null; } }
            public IFeatures Features { get { return null; } }
            public IShipSystems Systems { get { return null; } }
            public IShipEffects Effects { get { return null; } }
            public IShipSpecification Specification { get { return null; } }

            public void Dispose() { }
            public void Affect(Impact impact, IUnit source) { }
            public void AddPlatform(IWeaponPlatform platform) { }
            public void AddSystem(ISystem system) { }
            public void AddEffect(IShipEffect shipEffect) { }
			public void Broadcast(string message, Color color) {}
		}
    }
}
