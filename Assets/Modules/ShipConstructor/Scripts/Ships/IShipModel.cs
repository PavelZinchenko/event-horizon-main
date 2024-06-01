using System.Collections.Generic;
using Constructor.Model;
using Constructor.Ships.Modification;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public interface IShipModel
    {
        ItemId<Ship> Id { get; }
        ShipType ShipType { get; }
        SizeClass SizeClass { get; }
        ShipRarity ShipRarity { get; }
        string OriginalName { get; }
        Faction Faction { get; }
        IShipLayout Layout { get; }
        ImmutableCollection<Barrel> Barrels { get; }
        SpriteId ModelImage { get; }
        SpriteId IconImage { get; }
        float ModelScale { get; }
        float IconScale { get; }
        bool IsBionic { get; }
        public SizeClass MaxSatelliteSize { get; }
        public float MaxSatelliteModelSize { get; }

        ShipBaseStats Stats { get; }

        // TODO: remove
        Ship OriginalShip { get; }

        IItemCollection<IShipModification> Modifications { get; }
        LayoutModifications LayoutModifications { get; }

        bool DataChanged { get; set; }

        IShipModel Clone();
    }
}
