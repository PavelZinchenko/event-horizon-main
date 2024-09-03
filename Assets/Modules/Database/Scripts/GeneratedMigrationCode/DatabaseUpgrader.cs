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
    public partial class DatabaseUpgrader
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

            if (major == 0)
            {
                major = 1;
                minor = 0;
            }
            
            if (!IsValidVersion(major, minor))
                throw new DatabaseException($"invalid database version: {major}.{minor}");

            v1.Storage.DatabaseContent content1 = null;
            if (major <= 1)
            {
                content1 = new v1.Storage.DatabaseContent(_serializer, _storage);
                content1.VersionMajor = major;
                content1.VersionMinor = minor;
                var upgrader = new v1.DatabaseUpgrader(content1);
                upgrader.UpgradeMinor();
            }


            content1.Export(result);
        }

        private bool IsValidVersion(int major, int minor)
        {
            if (major == 1)
                return minor >= 0 && minor <= 7;

            return false;
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

                if (major != 1 || minor < 0 || minor > 7)
                    throw new DatabaseException($"invalid database version: {major}.{minor}");
            }

            public void UpgradeMinor()
            {
                if (Content.VersionMinor == 0)
                {
                    Migrate_0_1();
                    Content.VersionMinor = 1;
                }
                if (Content.VersionMinor == 1)
                {
                    Migrate_1_2();
                    Content.VersionMinor = 2;
                }
                if (Content.VersionMinor == 2)
                {
                    Migrate_2_3();
                    Content.VersionMinor = 3;
                }
                if (Content.VersionMinor == 3)
                {
                    Migrate_3_4();
                    Content.VersionMinor = 4;
                }
                if (Content.VersionMinor == 4)
                {
                    Migrate_4_5();
                    Content.VersionMinor = 5;
                }
                if (Content.VersionMinor == 5)
                {
                    Migrate_5_6();
                    Content.VersionMinor = 6;
                }
                if (Content.VersionMinor == 6)
                {
                    Migrate_6_7();
                    Content.VersionMinor = 7;
                }
            }

            partial void Migrate_0_1();
            partial void Migrate_1_2();
            partial void Migrate_2_3();
            partial void Migrate_3_4();
            partial void Migrate_4_5();
            partial void Migrate_5_6();
            partial void Migrate_6_7();

            protected Storage.DatabaseContent Content { get; }
        }
    }
}
