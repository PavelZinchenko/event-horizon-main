using System;
using System.Collections.Generic;
using System.IO;
using GameDatabase.Storage;
using NUnit.Framework;

namespace GameDatabase.Tests
{
    // Battle simulator phase 7, step 4 (risk 3): the duel's database hash with the original database, one mod, and the
    // same mod with one changed value.
    [TestFixture]
    public class DatabaseContentHashTests
    {
        private const string Ship = "{\"ItemType\": 6, \"Id\": 1, \"Name\": \"Ship\", \"Armor\": 100}";
        private const string Component = "{\"ItemType\": 1, \"Id\": 2, \"Name\": \"Laser\"}";
        private const string ModShip = "{\"ItemType\": 6, \"Id\": 1, \"Name\": \"Ship\", \"Armor\": 120}";

        [Test]
        public void EqualRecordsGiveEqualHashesWhateverTheFileNamesOrder()
        {
            var a = Folder(("a.json", Ship), ("b.json", Component));
            var b = Folder(("z.json", Component), ("sub/y.json", Ship));
            Assert.AreEqual(DatabaseContentHash.Length, Hash(null, a).Length);
            CollectionAssert.AreEqual(Hash(null, a), Hash(null, b));
        }

        [Test]
        public void LineEndingsDoNotCount()
        {
            var unix = Folder(("a.json", "{\n\"ItemType\": 6,\n\"Id\": 1\n}"));
            var windows = Folder(("a.json", "{\r\n\"ItemType\": 6,\r\n\"Id\": 1\r\n}"));
            CollectionAssert.AreEqual(Hash(null, unix), Hash(null, windows));
        }

        [Test]
        public void AModAndAChangedValueInTheModChangeTheHash()
        {
            var original = Folder(("a.json", Ship), ("b.json", Component));
            var mod = Folder(("a.json", ModShip));
            var changed = Folder(("a.json", ModShip.Replace("120", "121")));
            var withoutMod = Hash(null, original);
            var withMod = Hash(mod, original);
            CollectionAssert.AreNotEqual(withoutMod, withMod);
            CollectionAssert.AreNotEqual(withMod, Hash(changed, original));
            CollectionAssert.AreEqual(withMod, Hash(Folder(("x.json", ModShip)), Folder(("y.json", Component), ("z.json", Ship))));
        }

        // A mod that unloads the built-in catalog (null here) differs from the same mod over it.
        [Test]
        public void TheBuiltInCatalogCountsUnlessUnloaded()
        {
            var original = Folder(("a.json", Ship));
            var mod = Folder(("a.json", Component));
            CollectionAssert.AreNotEqual(Hash(mod, null), Hash(mod, original));
            CollectionAssert.AreNotEqual(Hash(mod, null), Hash(null, mod));
        }

        private static byte[] Hash(IDataStorage mod, IDataStorage builtIn) => DatabaseContentHash.Compute(mod, builtIn);

        private FolderDatabaseStorage Folder(params (string Name, string Json)[] files)
        {
            var path = Path.Combine(Path.GetTempPath(), "DatabaseContentHashTests", Guid.NewGuid().ToString("N"));
            foreach (var (name, json) in files)
            {
                var file = Path.Combine(path, name);
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllText(file, json);
            }
            _folders.Add(path);
            return new FolderDatabaseStorage(path);
        }

        [TearDown]
        public void DeleteFolders()
        {
            foreach (var folder in _folders)
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            _folders.Clear();
        }

        private readonly List<string> _folders = new List<string>();
    }
}
