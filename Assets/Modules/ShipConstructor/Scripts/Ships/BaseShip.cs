using System;
using System.Linq;
using Constructor.Satellites;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using Maths;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public abstract class BaseShip : IShip
    {
        protected BaseShip(IShipModel model, BehaviorTreeModel customAi = null)
        {
            _model = model ?? throw new System.ArgumentException("ShipWrapper is null");
            _experience = 0;
            CustomAi = customAi;
        }

        public ItemId<Ship> Id => _model.Id;

        public virtual string Name { get => _model.OriginalName; set => throw new InvalidOperationException(); }

        public IShipModel Model => _model;
        public ShipColorScheme ColorScheme => _colorScheme;

        public abstract IItemCollection<IntegratedComponent> Components { get; }

        public ISatelliteCollection Satellites => _satellites;

        public virtual DifficultyClass ExtraThreatLevel => DifficultyClass.Default;
        public BehaviorTreeModel CustomAi { get; }

        public Experience Experience
        {
            get => _experience;
            set
            {
                _experience = value;
                DataChanged = true;
            }
        }

        public virtual ShipBuilder CreateBuilder()
        {
            var builder = new ShipBuilder(this);
            var scale = Experience.PowerMultiplier;
            builder.Bonuses.ArmorPointsMultiplier *= scale;
            builder.Bonuses.ShieldPointsMultiplier *= scale;
            builder.Bonuses.DamageMultiplier *= scale;
            builder.Bonuses.RammingDamageMultiplier *= scale;

            // ShipBuilder.AllMountPoints selects the mount points that are part of the ship.
            var mountPoints = Model.Satellites;
            for (int i = 0; i < mountPoints.Count; ++i)
            {
                var satellite = _satellites[i];
                if (satellite != null)
                    builder.AddSatellite(satellite, mountPoints[i], i);
            }

            return builder;
        }

        public bool DataChanged
        {
            get
            {
                if (_dataChanged)
                    return true;
                if (_satellites.DataChanged)
                    return true;
                if (_model.DataChanged)
                    return true;
                if (_colorScheme.IsChanged)
                    return true;

                return false;
            }
            set
            {
                _dataChanged = value;
                if (_dataChanged)
                    return;

                _satellites.DataChanged = false;

                _model.DataChanged = false;
                _colorScheme.IsChanged = false;
            }
        }

        private Experience _experience;
        private readonly SatelliteCollection _satellites = new SatelliteCollection();
        private bool _dataChanged;
        private readonly ShipColorScheme _colorScheme = new ShipColorScheme();
        private readonly IShipModel _model;
    }
}
