using Constructor.Satellites;
using CommonComponents.Utils;
using GameDatabase.DataModel;

namespace Constructor.Ships
{
    public interface IShipPreset
    {
        Ship Ship { get; }
        string Name { get; set; }
        IItemCollection<IntegratedComponent> Components { get; }
        ISatelliteCollection Satellites { get; }

        [System.Obsolete("Use Satellites[0]. Remove after satellite mount points phase 9.")]
        ISatellite FirstSatellite { get; set; }
        [System.Obsolete("Use Satellites[1]. Remove after satellite mount points phase 9.")]
        ISatellite SecondSatellite { get; set; }
    }

    public class ShipPreset : IShipPreset
    {
        private readonly ObservableCollection<IntegratedComponent> _components = new ObservableCollection<IntegratedComponent>();

        public Ship Ship { get; }
        public string Name { get; set; }
        public IItemCollection<IntegratedComponent> Components => _components;
        public ISatelliteCollection Satellites { get; }

        [System.Obsolete("Use Satellites[0]")]
        public ISatellite FirstSatellite { get => Satellites[0]; set => Satellites[0] = value; }
        [System.Obsolete("Use Satellites[1]")]
        public ISatellite SecondSatellite { get => Satellites[1]; set => Satellites[1] = value; }

        public ShipPreset(Ship ship)
        {
            Ship = ship;
            Satellites = new SatelliteCollection();
        }
    }
}
