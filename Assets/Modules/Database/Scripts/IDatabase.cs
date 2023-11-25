using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Model;
using System;

namespace GameDatabase
{
    public partial interface IDatabase
    {
        public event Action DatabaseLoaded;

        void LookForMods();
        void LoadDefault();
        bool TryLoad(string id, out string error);
        IEnumerable<ModInfo> AvailableMods { get; }

        string Id { get; }
        string Name { get; }
        bool IsEditable { get; }

        IEnumerable<Faction> FactionsWithEmpty { get; }

        #region temporary members
        // TODO: remove this after database editor can edit builds
        void SaveShipBuild(ItemId<ShipBuild> id);
        void SaveSatelliteBuild(ItemId<SatelliteBuild> id);
        #endregion
    }

    public struct ModInfo
    {
        public ModInfo(string name, string id, string path)
        {
            Id = id;
            Name = name;
            Path = path;
        }

        public readonly string Id;
        public readonly string Name;
        public readonly string Path;

        public static readonly ModInfo Default = new ModInfo(string.Empty, string.Empty, string.Empty);
    }
}
