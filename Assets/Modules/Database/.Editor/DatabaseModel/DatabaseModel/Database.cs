using EditorDatabase.Storage;
using DatabaseMigration;

namespace EditorDatabase
{
    public partial class Database
    {
        public static Database MigrateFrom(IDataStorage storage)
        {
            var database = new Database(null);
            var upgrader = new DatabaseUpgrader(database._serializer, storage);
            upgrader.Upgrade(database._content);
            return database;
        }
    }
}

