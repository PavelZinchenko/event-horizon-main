using Combat.Component.Body;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Factory;
using Combat.Unit;
using Constructor;
using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    public class ClonningDevice : SystemBase, IDevice
    {
        public ClonningDevice(IShip mothership, DeviceStats stats, ShipFactory shipFactory, IShipSpecification shipSpec, EffectFactory effectFactory, int keyBinding)
            : base(keyBinding, stats.ControlButtonIcon)
        {
            _effectFactory = effectFactory;
            _effectPrefab = stats.EffectPrefab;

            MaxCooldown = stats.Cooldown;
            TimeFromLastUse = MaxCooldown * 0.8f;

            _shipFactory = shipFactory;
            _mothership = mothership;
            _shipSpec = new ShipSpecDecorator(shipSpec, 0.9f, 0.9f, 0.9f);
 
            _energyCost = stats.EnergyConsumption;
			DeviceClass = stats.DeviceClass;
        }

		public DeviceClass DeviceClass { get; }
		public override bool CanBeActivated => _clone == null && base.CanBeActivated;
        public override float Cooldown => _clone == null ? base.Cooldown : 1.0f;

		public IShip Clone => _clone;

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && CanBeActivated && _mothership.Stats.Energy.TryGet(_energyCost))
            {
                SpawnShip();
                InvokeTriggers(ConditionType.OnActivate);
            }

            if (_clone != null && _clone.State == UnitState.Destroyed)
            {
                _clone = null;
                TimeFromLastUse = 0;
            }
        }

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnDispose() {}

        private void SpawnShip()
        {
            if (_clone != null)
            {
                _clone.Vanish();
                _clone = null;
            }

            var random = new System.Random(GetHashCode());
            var rotation = random.Next(360);
            var direction = RotationHelpers.Direction(rotation);
            var position = _mothership.Body.WorldPosition() + _mothership.Body.Scale*direction;
            _clone = _shipFactory.CreateClone(_shipSpec, position, rotation, _mothership);
            _clone.Body.ApplyAcceleration(_mothership.Body.Velocity);

            _effectFactory.CreateEffect(_effectPrefab, _clone.Body)?.Run(0.5f, Vector2.zero, 0);
        }

        public void Deactivate() {}

        private IShip _clone;
        private readonly PrefabId _effectPrefab;
        private readonly EffectFactory _effectFactory;
        private readonly IShipSpecification _shipSpec;
        private readonly ShipFactory _shipFactory;
        private readonly IShip _mothership;
        private readonly float _energyCost;


        public class ShipSpecDecorator : IShipSpecification
        {
            public ShipSpecDecorator(IShipSpecification spec, float size, float attack, float defense)
            {
                _spec = spec;
                _stats = new ShipStatsWrapper(spec.Stats, size, attack, defense);
            }

            public ShipInfo Info => _spec.Info;
            public IShipStats Stats => _stats;
            public IEnumerable<IWeaponPlatformData> Platforms => _spec.Platforms;
            public IEnumerable<IDeviceData> Devices => _spec.Devices;
            public IEnumerable<IDroneBayData> DroneBays => Enumerable.Empty<IDroneBayData>();
			public BehaviorTreeModel CustomAi => _spec.CustomAi;

			private readonly IShipStats _stats;
            private readonly IShipSpecification _spec;
        }

        public class ShipStatsWrapper : IShipStats 
        {
            public ShipStatsWrapper(IShipStats stats, float size, float attack, float defense)
            {
                _stats = stats;
                _size = size;
                _attack = attack;
                _defense = defense;
            }

            public ColorScheme ShipColor => _stats.ShipColor;

            public ColorScheme TurretColor => _stats.TurretColor;

            public StatMultiplier DamageMultiplier => _stats.DamageMultiplier;

            public StatMultiplier ArmorMultiplier => _stats.ArmorMultiplier;

            public StatMultiplier ShieldMultiplier => _stats.ShieldMultiplier;

            public float ArmorPoints => _stats.ArmorPoints * _defense;

            public float EnergyPoints => _stats.EnergyPoints;

            public float ShieldPoints => _stats.ShieldPoints * _defense;

            public float EnergyRechargeRate => _stats.EnergyRechargeRate;

            public float ShieldRechargeRate => _stats.ShieldRechargeRate * _defense;

            public float ArmorRepairRate => _stats.ArmorRepairRate * _defense;

            public Layout Layout => _stats.Layout;

            public float Weight => _stats.Weight;

            public float EnginePower => _stats.EnginePower;

            public float TurnRate => _stats.TurnRate;

            public StatMultiplier WeaponDamageMultiplier => _stats.WeaponDamageMultiplier * _attack;

            public StatMultiplier WeaponFireRateMultiplier => _stats.WeaponFireRateMultiplier;

            public StatMultiplier WeaponEnergyCostMultiplier => _stats.WeaponEnergyCostMultiplier;

            public StatMultiplier WeaponRangeMultiplier => _stats.WeaponRangeMultiplier;

            public StatMultiplier DroneDamageMultiplier => new StatMultiplier();

            public StatMultiplier DroneDefenseMultiplier => new StatMultiplier();

            public StatMultiplier DroneSpeedMultiplier => new StatMultiplier();

            public StatMultiplier DroneRangeMultiplier => new StatMultiplier();

            public float EnergyAbsorption => _stats.EnergyAbsorption;

            public float RammingDamage => _stats.RammingDamage * _attack;

            public float RammingDamageMultiplier => _stats.RammingDamageMultiplier;

            public float ArmorRepairCooldown => _stats.ArmorRepairCooldown;

            public float EnergyRechargeCooldown => _stats.EnergyRechargeCooldown;

            public float ShieldRechargeCooldown => _stats.ShieldRechargeCooldown;

            public float EnergyResistance => _stats.EnergyResistance * _defense;

            public float KineticResistance => _stats.KineticResistance * _defense;

            public float ThermalResistance => _stats.ThermalResistance * _defense;

            public float EnergyAbsorptionPercentage => _stats.EnergyAbsorptionPercentage;

            public float KineticResistancePercentage => _stats.KineticResistancePercentage;

            public float EnergyResistancePercentage => _stats.EnergyResistancePercentage;

            public float ThermalResistancePercentage => _stats.ThermalResistancePercentage;

            public bool Autopilot => false;

            public float DroneBuildSpeed => _stats.DroneBuildSpeed;

            public float DroneBuildTime => _stats.DroneBuildTime;

            public GameDatabase.DataModel.Ship ShipModel => _stats.ShipModel;

            private readonly float _size;
            private readonly float _attack;
            private readonly float _defense;
            private readonly IShipStats _stats;
        }
    }
}
