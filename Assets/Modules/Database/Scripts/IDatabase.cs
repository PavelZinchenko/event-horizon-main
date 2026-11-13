using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Model;
using System;

namespace GameDatabase
{
    public partial interface IDatabase
    {
        event Action<Storage.IDataStorage> DatabaseLoading;
        event Action DatabaseLoaded;

        void LookForMods();
        bool TryAddModFromFile(string path);
        void LoadDefault();
        bool TryLoad(string id, out string error);
        IEnumerable<ModInfo> AvailableMods { get; }

        string Id { get; }
        string Name { get; }
        bool IsEditable { get; }

        IEnumerable<Faction> FactionsWithEmpty { get; }

        // SHA-256 of the loaded records (Storage.DatabaseContentHash). Computed on first use: it reads every record again.
        byte[] ContentHash { get; }

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
