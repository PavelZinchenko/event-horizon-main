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
    }

    public class ShipPreset : IShipPreset
    {
        private readonly ObservableCollection<IntegratedComponent> _components = new ObservableCollection<IntegratedComponent>();

        public Ship Ship { get; }
        public string Name { get; set; }
        public IItemCollection<IntegratedComponent> Components => _components;
        public ISatelliteCollection Satellites { get; }

        public ShipPreset(Ship ship)
        {
            Ship = ship;
            Satellites = new SatelliteCollection();
        }
    }
}
