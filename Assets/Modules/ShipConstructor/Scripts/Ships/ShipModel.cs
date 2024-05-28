using System.Collections.Generic;
using System.Linq;
using Constructor.Model;
using Constructor.Ships.Modification;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public sealed class ShipModel : IShipModel
    {
        public ShipModel(ShipBuild build, GameDatabase.IDatabase database)
        {
            _ship = build.Ship;
            Faction = build.BuildFaction != Faction.Empty ? build.BuildFaction : build.Ship.Faction;

            _layoutModifications = new LayoutModifications(build.Ship);
            if (build.ExtendedLayout)
                _layoutModifications.FullyUpgrade();

            _layoutModifications.DataChangedEvent += OnModificationsChanged;

            _modifications = new ObservableCollection<IShipModification>();
            if (database != null)
            {
                var factory = new ModificationFactory(database);
                var perk1 = build.Perks.Perk1.ToModificationType();
                var perk2 = build.Perks.Perk2.ToModificationType();
                var perk3 = build.Perks.Perk3.ToModificationType();
                if (perk1 != ModificationType.Empty) _modifications.Add(factory.Create(perk1));
                if (perk2 != ModificationType.Empty) _modifications.Add(factory.Create(perk2));
                if (perk3 != ModificationType.Empty) _modifications.Add(factory.Create(perk3));
            }

            _modifications.DataChangedEvent += OnModificationsChanged;
            OnModificationsChanged();
            DataChanged = false;
        }

        public ShipModel(Ship ship)
        {
            _ship = ship;
            Faction = ship.Faction;

            _layoutModifications = new LayoutModifications(ship);
            _layoutModifications.DataChangedEvent += OnModificationsChanged;

            _modifications = new ObservableCollection<IShipModification>();
            _modifications.DataChangedEvent += OnModificationsChanged;

            OnModificationsChanged();
            DataChanged = false;
        }

        public ShipModel(Ship ship, Faction faction) : this(ship)
        {
            if (faction != Faction.Empty)
                Faction = faction;
        }

        public ShipModel(Ship ship, IEnumerable<IShipModification> modifications, Faction faction) : this(ship, faction)
        {
            if (modifications != null)
                Modifications.Assign(modifications);
        }

        public ItemId<Ship> Id => _ship.Id;
        public ShipType ShipType => _ship.ShipType;
        public SizeClass SizeClass => _ship.SizeClass;
        public ShipRarity ShipRarity => _ship.ShipRarity;
        public string OriginalName => _ship.Name;
        public Faction Faction { get; private set; }
        public IShipLayout Layout => _stats.Layout;
        public ImmutableCollection<Barrel> Barrels => _stats.Barrels;
        public SpriteId ModelImage => _ship.ModelImage;
        public SpriteId IconImage => _ship.IconImage;
        public float ModelScale => _ship.ModelScale;
        public float IconScale => _ship.IconScale;
        public bool IsBionic => _ship.Features.Regeneration;// || _stats.RegenerationRate > 0;
        public SizeClass MaxSatelliteSize => _stats.MaxSatelliteSize;
        public IEnumerable<Device> BuiltinDevices => _stats.BuiltinDevices;

        public event System.Action DataChangedEvent;

        public bool DataChanged 
        { 
            get => _datachanged;
            set
            {
                if (_datachanged == value) return;
                _datachanged = value;

                if (_datachanged)
                    DataChangedEvent?.Invoke();
            }
        }

        public Ship OriginalShip => _ship;

        public ShipBaseStats Stats => _stats;
        public LayoutModifications LayoutModifications => _layoutModifications;
        public IItemCollection<IShipModification> Modifications => _modifications;

        public IShipModel Clone()
        {
            var model = new ShipModel(_ship);
            model.Modifications.Assign(Modifications);
            model.Faction = Faction;
            model.LayoutModifications.Deserialize(LayoutModifications.Serialize().ToArray());
            return model;
        }

        private void OnModificationsChanged()
        {
			_stats = new ShipBaseStats
			{
				ShipWeightMultiplier = new StatMultiplier(_ship.Features.ShipWeightBonus),
				EquipmentWeightMultiplier = new StatMultiplier(_ship.Features.EquipmentWeightBonus),
				VelocityMultiplier = new StatMultiplier(_ship.Features.VelocityBonus),
				TurnRateMultiplier = new StatMultiplier(_ship.Features.TurnRateBonus),
				ShieldMultiplier = new StatMultiplier(_ship.Features.ShieldBonus),
				ArmorMultiplier = new StatMultiplier(_ship.Features.ArmorBonus),
				EnergyMultiplier = new StatMultiplier(_ship.Features.EnergyBonus),
                DroneAttackMultiplier = new StatMultiplier(_ship.Features.DroneAttackBonus),
                DroneDefenseMultiplier = new StatMultiplier(_ship.Features.DroneDefenseBonus),
                DroneBuildSpeedMultiplier = new StatMultiplier(_ship.Features.DroneBuildSpeedBonus),
                MaxSatelliteSize = _ship.SizeClass,
				Layout = _layoutModifications.BuildLayout(),
				BuiltinDevices = _ship.Features.BuiltinDevices,
				Barrels = _ship.Barrels,
            };

			bool barrelsCopied = false;
			foreach (var modification in _modifications)
			{
				if (modification.ChangesBarrels && !barrelsCopied)
				{
					_stats.Barrels = new ImmutableCollection<Barrel>(_ship.Barrels.Select(Barrel.Clone));
					barrelsCopied = true;
				}

				modification.Apply(ref _stats);
			}

            DataChanged = true;
        }

        private bool _datachanged;
		private readonly Ship _ship;
        private ShipBaseStats _stats;
        private readonly LayoutModifications _layoutModifications;
        private readonly ObservableCollection<IShipModification> _modifications;
    }
}
