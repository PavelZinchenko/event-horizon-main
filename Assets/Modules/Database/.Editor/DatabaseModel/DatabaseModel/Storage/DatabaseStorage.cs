using System;
using System.IO;
using EditorDatabase.Model;

namespace EditorDatabase.Storage
{
    public partial interface IDataStorage 
    {
        Version Version { get; }
    }

    public class DatabaseStorage : IDataStorage
    {
        public DatabaseStorage(string path)
        {
            _path = path;
        }

        public Version LoadDatabaseVersion()
        {
            var serializer = new JsonSerializer();
            var info = new DirectoryInfo(_path);
            foreach (var fileInfo in info.GetFiles("*.json", SearchOption.AllDirectories))
            {
                var data = File.ReadAllText(fileInfo.FullName);
                var settings = serializer.FromJson<Serializable.DatabaseSettingsSerializable>(data);
                if (settings.ItemType != Enums.ItemType.DatabaseSettings) continue;
                return Version = new Version(settings.DatabaseVersion, settings.DatabaseVersionMinor);
            }

            return Version = new Version(1,0);
        }

        public Version Version { get; private set; }

        public void LoadContent(IContentLoader loader)
        {
            var info = new DirectoryInfo(_path);
            var itemCount = 0;
            foreach (var fileInfo in info.GetFiles("*", SearchOption.AllDirectories))
            {
                var file = fileInfo.FullName;
                var path = file.Substring(info.FullName.Length + 1);

                if (fileInfo.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    fileInfo.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    fileInfo.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    loader.LoadImage(Path.GetFileNameWithoutExtension(file), new ImageData(file));
                }
                else if (fileInfo.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO
                }
                else if (fileInfo.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    var xmlData = File.ReadAllText(file);
                    loader.LoadLocalization(Path.GetFileNameWithoutExtension(file), xmlData);
                }
                else if (fileInfo.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var data = File.ReadAllText(file);
                    loader.LoadJson(path, data);
                    itemCount++;
                }
            }

            if (itemCount == 0)
                throw new FileNotFoundException("No database files found in " + _path);
        }

        public void SaveJson(string name, string data)
        {
            var fullName = Path.Combine(_path, name);
            File.WriteAllText(EnsurePathExists(fullName), data);
        }

        public string Name { get; }
        public string Id { get; }

        private static string EnsurePathExists(string path)
        {
            var directory = Directory.GetParent(path).FullName;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return path;
        }

        private readonly string _path;
    }

    public readonly struct Version
    {
        public Version(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public int Compare(int major, int minor)
        {
            if (Major == major) return Minor - minor;
            return Major - major;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }

        public readonly int Major;
        public readonly int Minor;
    }
}
