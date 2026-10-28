using GameDatabase.Storage;
using UnityEngine;

namespace GameDatabase
{
    // Creates a Database with the engine-side default storage, JSON serializer and mods folder.
    public static class UnityDatabaseFactory
    {
        private const string DefaultPath = "Database";

        public static Database Create()
        {
            var serializer = new UnityJsonSerializer();
            DatabaseMigration.MigrationResources.StorageProvider = path => new ResourceDatabaseStorage(path);
            DatabaseMigration.MigrationResources.Serializer = serializer;
            return new Database(new ResourceDatabaseStorage(DefaultPath), serializer, ModsPath);
        }

        private static string ModsPath
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return Application.persistentDataPath + "/Mods/";
#elif UNITY_STANDALONE || UNITY_EDITOR
                return Application.dataPath + "/../Mods/";
#else
                return string.Empty;
#endif
            }
        }
    }
}
