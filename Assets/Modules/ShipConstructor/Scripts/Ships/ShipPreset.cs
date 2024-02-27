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
        ISatellite FirstSatellite { get; set; }
        ISatellite SecondSatellite { get; set; }
    }

    public class ShipPreset : IShipPreset
    {
        private readonly ObservableCollection<IntegratedComponent> _components = new ObservableCollection<IntegratedComponent>();

        public Ship Ship { get; }
        public string Name { get; set; }
        public IItemCollection<IntegratedComponent> Components => _components;
        public ISatellite FirstSatellite { get; set; }
        public ISatellite SecondSatellite { get; set; }

        public ShipPreset(Ship ship)
        {
            Ship = ship;
        }
    }
}
