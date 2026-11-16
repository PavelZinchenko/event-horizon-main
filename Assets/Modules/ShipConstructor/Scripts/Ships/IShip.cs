using Constructor.Satellites;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using CommonComponents.Utils;
using System.Collections.Generic;

namespace Constructor.Ships
{
    public interface IShip
    {
        ItemId<Ship> Id { get; }
        string Name { get; set; }
        ShipColorScheme ColorScheme { get; }

        IShipModel Model { get; }

        IItemCollection<IntegratedComponent> Components { get; }
        ISatelliteCollection Satellites { get; }

		DifficultyClass ExtraThreatLevel { get; }
		BehaviorTreeModel CustomAi { get; }

		Maths.Experience Experience { get; set; }

        ShipBuilder CreateBuilder();

        bool DataChanged { get; set; }
    }

    public interface ISatelliteCollection : IEnumerable<ISatellite>
    {
        int UsedMountPoints { get; }
        ISatellite this[int index] { get; set; }
    }

    public static class SatelliteCollectionExtensions 
    {
        public static void Assign(this ISatelliteCollection collection, IEnumerable<ISatellite> satellites)
        {
            int index = 0;
            foreach (var satellite in satellites)
                collection[index++] = satellite;
            for (int i = index; i < collection.UsedMountPoints; ++i)
                collection[i] = null;
        }

        public static void CopyFrom(this ISatelliteCollection collection, IEnumerable<ISatellite> satellites)
        {
            int index = 0;
            foreach (var satellite in satellites)
                collection[index++] = satellite.CreateCopy();
            for (int i = index; i < collection.UsedMountPoints; ++i)
                collection[i] = null;
        }
    }
}
