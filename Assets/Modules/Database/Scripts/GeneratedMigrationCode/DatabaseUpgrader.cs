//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using GameDatabase.Storage;

namespace DatabaseMigration
{
    public class DatabaseUpgrader
    {
        public DatabaseUpgrader(IJsonSerializer jsonSerializer, IDataStorage storage)
        {
            _serializer = jsonSerializer;
            _storage = storage;
        }

        public void Upgrade(IContentLoader result)
        {
            var major = _storage.Version.Major;
            var minor = _storage.Version.Minor;

            v1.Storage.DatabaseContent content1 = null;
            if (major <= 1)
            {
                content1 = new v1.Storage.DatabaseContent(_serializer, _storage);
                var upgrader = new v1.DatabaseUpgrader(content1);
                upgrader.UpgradeMinor();
            }


            if (major <= 0 || major > 1)
                throw new DatabaseException($"invalid database version: {major}.{minor}");

            content1.Export(result);
        }

        private readonly IDataStorage _storage;
        private readonly IJsonSerializer _serializer;
    }

    namespace v1
    {
        public partial class DatabaseUpgrader
        {
            public DatabaseUpgrader(Storage.DatabaseContent content)
            {
                Content = content;
                var major = content.VersionMajor;
                var minor = content.VersionMinor;

                if (major != 1 || minor < 0 || minor > 1)
                    throw new DatabaseException($"invalid database version: {major}.{minor}");
            }

            public void UpgradeMinor()
            {
                if (Content.VersionMinor == 0)
                {
                    Migrate_0_1();
                    Content.VersionMinor = 1;
                }
            }

            partial void Migrate_0_1();

            protected Storage.DatabaseContent Content { get; }
        }
    }
}
