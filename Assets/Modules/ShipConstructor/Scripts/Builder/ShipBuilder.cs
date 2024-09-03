using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Component;
using Constructor.Detail;
using Constructor.Model;
using Constructor.Satellites;
using Constructor.Ships;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;
using Utilites.Collections;

namespace Constructor
{
	public class ShipBuilder
	{
		public ShipBuilder(ShipBuild ship)
            : this(new ShipModel(ship, null), ship.Components.Select(ComponentExtensions.FromDatabase))
		{
            var boost = 1f + 0.5f * (int)ship.DifficultyClass;
            Bonuses.DamageMultiplier *= boost;
            Bonuses.ArmorPointsMultiplier *= boost;
            Bonuses.ShieldPointsMultiplier *= boost;
            //Bonuses.RammingDamageMultiplier *= boost;
            ShipVisualDifficulty = ship.DifficultyClass;
            CustomAi = ship.CustomAI;
        }

        public ShipBuilder(IShipModel model, IEnumerable<IntegratedComponent> components)
	    {
	        _ship = model;
	        _shipComponents = new List<IntegratedComponent>(components);
        }

        public ShipBuilder(IShip ship)
            : this(ship.Model, ship.Components)
		{
		    ShipVisualLevel = ship.Experience.Level;
			CustomAi = ship.CustomAi;
            ShipVisualDifficulty = ship.ExtraThreatLevel;
            TurretColor = ShipColor = new ColorScheme(ship.ColorScheme.Color, ship.ColorScheme.Hue, ship.ColorScheme.Saturation);
		}

        public int ShipVisualLevel { get; set; } // Doesn't affect stats, pure visual
        public DifficultyClass ShipVisualDifficulty { get; set; } = DifficultyClass.Default; // Doesn't affect stats, pure visual
        public int ExtraDroneBayCapacity { get; set; }
        public ShipBonuses Bonuses;
	    public ColorScheme ShipColor;
	    public ColorScheme TurretColor;
		public StatMultiplier SizeMultiplier;

        public BehaviorTreeModel CustomAi { get; set; }

		//public List<IntegratedComponent> ShipComponents { get { return _shipComponents; } }

		public IComponentConverter Converter { get { return _converter; } set { _converter = value ?? DefaultComponentConverter.Instance; } }

		public void AddSatellite(ISatellite spec, CompanionLocation location)
		{
			_satellites.Add(new KeyValuePair<ISatellite, CompanionLocation>(spec, location));
		}

		public IShipSpecification Build(ShipSettings settings)
		{
			var size = Size;
		    var data = new ShipBuilderResult();

		    var stats = new ShipStatsCalculator(_ship.OriginalShip, settings);
		    stats.Bonuses = Bonuses;
		    stats.ShipColor = ShipColor;
		    stats.TurretColor = TurretColor;
		    stats.BaseStats = _ship.Stats;
			stats.SizeMultiplier = SizeMultiplier;

            data.Stats = stats;
			data.Info = new ShipInfo(_ship.Id, ShipVisualDifficulty, ShipVisualLevel, size);
			data.CustomAi = CustomAi;

            foreach (var platform in GetPlatforms(settings))
                data.AddPlatform(platform);

            foreach (var item in stats.BuiltinDevices)
		        data.AddDevice(new DeviceData(item.Stats, item.Stats.ActivationType == ActivationType.Manual ? 5 : -1));

            var limitedComponents = new SimpleInventory<GameDatabase.DataModel.Component>();
		    var componentTags = new SimpleInventory<ComponentGroupTag>();
            var energyDependentComponents = new List<(ComponentSpec spec, IComponent component)>();
            var energyIndependentComponents = new List<(ComponentSpec spec, IComponent component)>();
            foreach (var item in Components)
            {
                var tag = item.Info.Data.Restrictions.ComponentGroupTag;
                if (tag != null)
                {
                    if (componentTags.GetQuantity(tag) >= tag.MaxInstallableComponents)
                        continue;
                    else
                        componentTags.Add(tag);
                }

                var maxCount = item.Info.Data.Restrictions.MaxComponentAmount;
                if (maxCount > 0)
                {
                    if (limitedComponents.GetQuantity(item.Info.Data) >= maxCount)
                        continue;
                    else
                        limitedComponents.Add(item.Info.Data);
                }

                var component = item.Info.CreateComponent(_ship.Layout.CellCount);
                if (component == null || !component.IsSuitable(_ship))
                    continue;

                if (component.RequiresEnergyToInstall())
                    energyDependentComponents.Add((item,component));
                else
                    energyIndependentComponents.Add((item, component));
            }

            foreach (var item in energyIndependentComponents)
                InstallComponent(ref data, ref stats, item.spec, item.component, false);
            foreach (var item in energyDependentComponents)
                InstallComponent(ref data, ref stats, item.spec, item.component, true);

            data.ApplyStats(stats);

			return data;
		}

        private void InstallComponent(ref ShipBuilderResult data, ref ShipStatsCalculator stats, in ComponentSpec item, IComponent component, bool ifEnoughEnergy)
        {
            var componentStats = component.GetStats();

            if (ifEnoughEnergy)
            {
                if (stats.EnergyRechargeRate < componentStats.EnergyConsumption)
                {
                    stats.EquipmentStats.AddNegativeStatsOnly(componentStats);
                    return;
                }
            }

            stats.EquipmentStats.AddStats(componentStats);

            foreach (var spec in component.Devices)
                data.AddDevice(new DeviceData(spec, item.KeyBinding));
            foreach (var spec in component.DroneBays)
            {
                var droneBayStats = spec.Key;
                droneBayStats.Capacity += ExtraDroneBayCapacity;
                data.AddDroneBay(new DroneBayData(droneBayStats, spec.Value, item.KeyBinding, (DroneBehaviour)item.Behaviour));
            }

            if (item.BarrelId < 0)
                return;
            var platform = data.FindPlatform(item.BarrelId);
            if (platform == null)
                return;

            component.UpdateWeaponPlatform(platform);
            foreach (var spec in component.Weapons)
            {
                platform.AddWeapon(spec.Weapon, spec.Ammunition, spec.StatModifier, item.KeyBinding);
            }

            foreach (var spec in component.WeaponsObsolete)
                platform.AddWeapon(spec.Key, spec.Value, item.KeyBinding);
        }

        private int Size
		{
			get
			{
				var size = 0;
				foreach (var item in _satellites)
					size += item.Key.Information.Layout.CellCount;

				return _ship.Layout.CellCount + size/2;
			}
		}

		private IEnumerable<WeaponPlatform> GetPlatforms(ShipSettings settings)
		{
			var id = 0;
		    foreach (var barrel in _ship.Barrels)
		    {
		        var platform = new WeaponPlatform(barrel) { BarrelId = id++ };
		        yield return platform;
		    }

            foreach (var item in _satellites)
			{
				var barrels = item.Key.Information.Barrels;
				var count = barrels.Count();

                if (count > 1)
                    throw new ArgumentException("companions should have one barrel");

                if (count == 0)
                    yield return new WeaponPlatform(Barrel.Empty) { Companion = new CompanionSpec(item.Key.Information, item.Value, settings) };
                else
                    yield return new WeaponPlatform(barrels.First()) { Companion = new CompanionSpec(item.Key.Information, item.Value, settings), BarrelId = id++ };
            }
		}

		private IEnumerable<ComponentSpec> Components
		{
			get
			{
				int barrel = 0;
			    foreach (var component in _shipComponents)
			        yield return _converter.Process(component, barrel);

			    barrel += _ship.Barrels.Count;

				foreach (var item in _satellites)
				{
					foreach (var component in item.Key.Components)
                        yield return _converter.Process(component, barrel);

                    barrel += item.Key.Information.Barrels.Count;
				}
			}
		}

        private IComponentConverter _converter = DefaultComponentConverter.Instance;
        private readonly IShipModel _ship;
		private readonly List<IntegratedComponent> _shipComponents;
		private readonly List<KeyValuePair<ISatellite, CompanionLocation>> _satellites = new List<KeyValuePair<ISatellite, CompanionLocation>>();
	}

	public class DeviceData : IDeviceData
	{
		public DeviceData(DeviceStats spec, int key)
		{
			Device = spec;
			KeyBinding = spec.ActivationType.ValidateKey(key);
		}

		public DeviceStats Device { get; private set; }
		public int KeyBinding { get; private set; }
    }

	public class DroneBayData : IDroneBayData
	{
		public DroneBayData(in DroneBayStats spec, ShipBuild drone, int key, DroneBehaviour behaviour)
		{
			_droneBay = spec;
			Drone = drone;
			KeyBinding = spec.ActivationType.ValidateKey(key);
		    Behaviour = behaviour;
		}

		public DroneBayStats DroneBay
		{
			get
			{
				var stats = _droneBay; 
				stats.DamageMultiplier += DroneDamageModifier;
                stats.DefenseMultiplier += DroneDefenseModifier;
                stats.SpeedMultiplier += DroneSpeedModifier;
                stats.Range *= 1.0f + DroneRangeModifier;

			    stats.DamageMultiplier *= TotalDamageMultiplier;
			    stats.DefenseMultiplier *= TotalDefenseMultiplier;

				return stats;
			}
		}
		public ShipBuild Drone { get; private set; }
		public int KeyBinding { get; private set; }
	    public DroneBehaviour Behaviour { get; private set; }

        public float TotalDamageMultiplier { get; set; }
        public float TotalDefenseMultiplier { get; set; }

        public float DroneDamageModifier { get; set; }
        public float DroneDefenseModifier { get; set; }
        public float DroneSpeedModifier { get; set; }
		public float DroneRangeModifier { get; set; }

        private readonly DroneBayStats _droneBay;
	}

	namespace Detail
	{
		public class WeaponPlatform : IWeaponPlatformData, IWeaponPlatformStats
		{
			public WeaponPlatform(Barrel barrel)
			{
				Position = barrel.Position;
				Rotation = barrel.Rotation;
				Offset = barrel.Offset;

			    _rotationArc = barrel.AutoAimingArc;
			    RotationSpeed = barrel.RotationSpeed > 0 ? barrel.RotationSpeed : 270;
                DamageMultiplier = 1;
				FireRateMultiplier = 1;
				EnergyConsumptionMultiplier = 1;
			    BarrelId = -1;
			    Size = barrel.Size;
			    Image = barrel.Image;
			}

		    public void ChangeAutoAimingArc(float angle)
		    {
		        _rotationArc = Mathf.Max(AutoAimingArc, angle);
		    }

            public void ChangeTurnRate(float deltaAngle)
			{
				RotationSpeed += deltaAngle;
			}

            public Vector2 Position { get; }
			public float Rotation { get; }
			public float Offset { get; }
            public float Size { get; }
            public SpriteId Image { get; }
			public float AutoAimingArc => RotationSpeed > 0 ? _rotationArc : 0;

            public float RotationSpeed { get; private set; }
			public ICompanionData Companion { get; set; }

		    public int BarrelId { get; set; }
		    public float DamageMultiplier { get; set; }
			public float FireRateMultiplier { get; set; }
			public float EnergyConsumptionMultiplier { get; set; }
			public float RangeMultiplier { get; set; }

            private float _rotationArc;

            public IEnumerable<IWeaponData> Weapons
		    {
		        get
		        {
		            foreach (var item in _weapons)
		            {
		                item.DamageMultiplier = DamageMultiplier;
		                item.FireRateMultiplier = FireRateMultiplier;
		                item.EnergyConsumptionMultiplier = EnergyConsumptionMultiplier;
		                item.RangeMultiplier = RangeMultiplier;
		                yield return item;
		            }
		        }
            }

		    public IEnumerable<IWeaponDataObsolete> WeaponsObsolete
			{
				get
				{
					foreach (var item in _weaponsObsolete)
					{
						item.DamageMultiplier = DamageMultiplier;
						item.FireRateMultiplier = FireRateMultiplier;
						item.EnergyConsumptionMultiplier = EnergyConsumptionMultiplier;
						item.RangeMultiplier = RangeMultiplier;
						yield return item;
					}
				}
			}
			
			public void AddWeapon(in WeaponStats weapon, in AmmunitionObsoleteStats ammunition, int key)
			{
				_weaponsObsolete.Add(new WeaponDataObsolete(weapon, ammunition, key));
			}

		    public void AddWeapon(Weapon weapon, Ammunition ammunition, in WeaponStatModifier stats, int key)
		    {
		        _weapons.Add(new WeaponData(weapon, ammunition, stats, key));
		    }

            private readonly List<WeaponData> _weapons = new List<WeaponData>();
	        private readonly List<WeaponDataObsolete> _weaponsObsolete = new List<WeaponDataObsolete>();

		    public class WeaponData : IWeaponData
		    {
		        public WeaponData(Weapon weapon, Ammunition ammunition, in WeaponStatModifier stats, int key)
		        {
		            _weapon = weapon;
		            _ammunition = ammunition;
		            _stats = stats;
		            KeyBinding = weapon.Stats.ActivationType.ValidateKey(key);
		        }

		        public Weapon Weapon { get { return _weapon; } }
		        public Ammunition Ammunition { get { return _ammunition; } }

		        public WeaponStatModifier Stats
		        {
		            get
		            {
		                var stats = _stats;
		                stats.DamageMultiplier *= DamageMultiplier;
		                stats.EnergyCostMultiplier *= EnergyConsumptionMultiplier;
		                stats.FireRateMultiplier *= FireRateMultiplier;
		                stats.RangeMultiplier *= RangeMultiplier;
		                return stats;
		            }
		        }

		        public int KeyBinding { get; private set; }

		        public float DamageMultiplier { get; set; }
		        public float FireRateMultiplier { get; set; }
		        public float EnergyConsumptionMultiplier { get; set; }
		        public float RangeMultiplier { get; set; }

		        private readonly Weapon _weapon;
		        private readonly Ammunition _ammunition;
		        private readonly WeaponStatModifier _stats;
		    }

            public class WeaponDataObsolete : IWeaponDataObsolete
            {
				public WeaponDataObsolete(in WeaponStats weapon, in AmmunitionObsoleteStats ammunition, int key)
				{
					_weapon = weapon;
				    _ammunition = ammunition;
					KeyBinding = weapon.ActivationType.ValidateKey(key);
				}

			    public WeaponStats Weapon
			    {
			        get
			        {
			            var stats = _weapon;
                        stats.FireRate *= FireRateMultiplier;
			            return stats;
			        }
                }

                public AmmunitionObsoleteStats Ammunition 
				{
					get
					{
						var stats = _ammunition;
						stats.Damage *= DamageMultiplier;
						stats.EnergyCost *= EnergyConsumptionMultiplier;
						stats.Range *= RangeMultiplier;
						return stats;
					}
				}
				
				public int KeyBinding { get; private set; }
				
				public float DamageMultiplier { get; set; }
				public float FireRateMultiplier { get; set; }
				public float EnergyConsumptionMultiplier { get; set; }
				public float RangeMultiplier { get; set; }

                private readonly WeaponStats _weapon;
                private readonly AmmunitionObsoleteStats _ammunition;
            }
		}
		
		public class ShipBuilderResult : IShipSpecification
		{
            private readonly List<WeaponPlatform> _platforms = new List<WeaponPlatform>();
            private readonly List<DeviceData> _devices = new List<DeviceData>();
            private readonly List<DeviceData> _clonningCenters = new List<DeviceData>();
            private readonly List<DroneBayData> _droneBays = new List<DroneBayData>();
            
            public ShipInfo Info { get; set; }
			public IShipStats Stats { get; set; }
            public IEnumerable<IWeaponPlatformData> Platforms => _platforms.Cast<IWeaponPlatformData>();
            public IEnumerable<IDeviceData> Devices => _devices.Cast<IDeviceData>();
            public IEnumerable<IDeviceData> ClonningCenters => _clonningCenters.Cast<IDeviceData>();
            public IEnumerable<IDroneBayData> DroneBays => _droneBays.Cast<IDroneBayData>();
            public BehaviorTreeModel CustomAi { get; set; }

            public WeaponPlatform FindPlatform(int barrelId)
            {
                return _platforms.Find(item => item.BarrelId == barrelId);
            }

            public void AddPlatform(WeaponPlatform data)
            {
                _platforms.Add(data);
            }

            public void AddDevice(DeviceData data)
            {
                if (data.Device.DeviceClass == DeviceClass.ClonningCenter)
                    _clonningCenters.Add(data);
                else
                    _devices.Add(data);
            }

            public void AddDroneBay(DroneBayData data)
            {
                _droneBays.Add(data);
            }
            
            public void ApplyStats(ShipStatsCalculator stats)
            {
                foreach (var platform in _platforms)
                {
                    platform.DamageMultiplier = stats.WeaponDamageMultiplier.Value;
                    platform.FireRateMultiplier = stats.WeaponFireRateMultiplier.Value;
                    platform.EnergyConsumptionMultiplier = stats.WeaponEnergyCostMultiplier.Value;
                    platform.RangeMultiplier = stats.WeaponRangeMultiplier.Value;
                }

                foreach (var droneBay in _droneBays)
                {
                    droneBay.TotalDamageMultiplier = stats.Bonuses.DamageMultiplier.Value;
                    droneBay.TotalDefenseMultiplier = stats.Bonuses.ArmorPointsMultiplier.Value;

                    droneBay.DroneDamageModifier = stats.DroneDamageMultiplier.Bonus;
                    droneBay.DroneDefenseModifier = stats.DroneDefenseMultiplier.Bonus;
                    droneBay.DroneSpeedModifier = stats.DroneSpeedMultiplier.Bonus;
                    droneBay.DroneRangeModifier = stats.DroneRangeMultiplier.Bonus;
                }
            }
        }

        public class CompanionSpec : ICompanionData
		{
			public CompanionSpec(Satellite satellite, CompanionLocation location, ShipSettings settings)
			{
				Satellite = satellite;
				Location = location;
				Weight = Satellite.Layout.CellCount * settings.DefaultWeightPerCell / 1000f;
			}
			
			public Satellite Satellite { get; private set; }
			public CompanionLocation Location { get; private set; }
			public float Weight { get; private set; }
		}		
	}	
}
