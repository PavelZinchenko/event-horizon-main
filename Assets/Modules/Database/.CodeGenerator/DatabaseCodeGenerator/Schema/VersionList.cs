using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace DatabaseCodeGenerator.Schema
{
    public class VersionList
    {
        public VersionList(string filename)
        {
            LoadResources(filename);
        }

        public IEnumerable<SchemaVersion> Items => _schemaList;
        
        public bool TryGetLatest(out SchemaVersion version)
        {
            if (_schemaList.Count == 0)
            {
                version = new SchemaVersion();
                return false;
            }

            version = _schemaList[_schemaList.Count - 1];
            return true;
        }

        private void LoadResources(string filename)
        {
            var serializer = new XmlSerializer(typeof(XmlVersionList));

            try
            {
                var data = File.ReadAllText(filename);

                XmlVersionList versionList;
                using (var reader = new System.IO.StringReader(data))
                    versionList = serializer.Deserialize(reader) as XmlVersionList;

                foreach (var item in versionList.members)
                {
                    if (!int.TryParse(item.major, out var major) || !int.TryParse(item.minor, out var minor)) continue;
                    _schemaList.Add(new SchemaVersion { Path = item.name, Major = major, Minor = minor });
                }

                _schemaList.Sort(Compare);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to load " + filename + ": " + e.Message);
                throw;
            }
        }

        private static int Compare(SchemaVersion first, SchemaVersion second)
        {
            if (first.Major == second.Major)
                return first.Minor - second.Minor;
            else
                return first.Major - second.Major;
        }

        private readonly List<SchemaVersion> _schemaList = new List<SchemaVersion>();
    }

    public struct SchemaVersion
    {
        public string Path;
        public int Major;
        public int Minor;

        public string ToNamespace() => $"v{Major}";
        public bool IsNull => Major <= 0;
    }
}
