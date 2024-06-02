using Constructor.Satellites;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public interface IShip
    {
        ItemId<Ship> Id { get; }
        string Name { get; set; }
        ShipColorScheme ColorScheme { get; }

        IShipModel Model { get; }

        IItemCollection<IntegratedComponent> Components { get; }
        ISatellite FirstSatellite { get; set; }
        ISatellite SecondSatellite { get; set; }

		DifficultyClass ExtraThreatLevel { get; }
		BehaviorTreeModel CustomAi { get; }

		Maths.Experience Experience { get; set; }

        ShipBuilder CreateBuilder();

        bool DataChanged { get; set; }
    }
}
