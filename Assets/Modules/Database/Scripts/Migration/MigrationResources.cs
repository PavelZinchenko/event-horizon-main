using System;
using GameDatabase.Storage;

namespace DatabaseMigration
{
    // Built-in data that old migrations load (Unity Resources/DatabaseMigration/...).
    // Database.Unity sets the provider in UnityDatabaseFactory.Create().
    public static class MigrationResources
    {
        public static Func<string, IDataStorage> StorageProvider { get; set; }

        public static IDataStorage GetStorage(string path)
        {
            if (StorageProvider == null)
                throw new InvalidOperationException("MigrationResources.StorageProvider is not set, cannot load " + path);

            return StorageProvider(path);
        }
    }
}
