using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseMigration.v1;
using DatabaseMigration.v1.Serializable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GameDatabase.Editor
{
    // Converts the obsolete ammunition of the built-in database (battle simulator phase 4, step 4c, D2). Each obsolete
    // record becomes a new record with the same ID and file name in Ammunition/; child area bullets, bullet prefabs and
    // visual effects get new files. The obsolete file is deleted; component files and the database version do not change.
    // A second run finds no obsolete records and does nothing.
    public static class AmmunitionConversionRunner
    {
        private const string DatabaseFolder = "Assets/Modules/Database/Resources/Database";
        private const string AmmunitionFolder = DatabaseFolder + "/Ammunition";

        // Images that the new records need in the folders the ResourceLocator reads (mapping table, V1).
        private static readonly (string From, string To)[] ImageCopies =
        {
            ("Assets/Resources/Textures/ball.png", "Assets/Resources/Textures/Bullets/ball_soft.png"),
            ("Assets/Resources/Textures/Bullets/hotspot1.png", "Assets/Resources/Textures/Effects/hotspot1.png"),
            ("Assets/Resources/Textures/Objects/atmosphere.png", "Assets/Resources/Textures/Effects/atmosphere.png"),
        };

        [MenuItem("Tools/Database/Convert Obsolete Ammunition")]
        public static void Run()
        {
            var report = new StringBuilder();
            try
            {
                var written = Convert(report);
                CopyImages(report);
                AssetDatabase.Refresh();
                Debug.Log($"AmmunitionConversionRunner: {written} files written.\n{report}");
            }
            catch (Exception e)
            {
                AssetDatabase.Refresh();
                Debug.LogError($"AmmunitionConversionRunner: {e.Message}\n{report}");
            }
        }

        private static int Convert(StringBuilder report)
        {
            var obsolete = new List<(string Path, AmmunitionObsoleteSerializable Item)>();
            var ammunitionIds = new HashSet<int>();
            int maxAmmunition = 0, maxPrefab = 0, maxEffect = 0;

            foreach (var path in Directory.GetFiles(DatabaseFolder, "*.json", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(path);
                JObject json;
                try { json = JObject.Parse(text); }
                catch (JsonException) { continue; }

                var type = (int?)json["ItemType"];
                var id = (int?)json["Id"] ?? 0;
                switch ((Enums.ItemType?)type)
                {
                    case Enums.ItemType.AmmunitionObsolete:
                        obsolete.Add((path.Replace('\\', '/'), JsonConvert.DeserializeObject<AmmunitionObsoleteSerializable>(text)));
                        break;
                    case Enums.ItemType.Ammunition:
                        ammunitionIds.Add(id);
                        maxAmmunition = Math.Max(maxAmmunition, id);
                        break;
                    case Enums.ItemType.BulletPrefab:
                        maxPrefab = Math.Max(maxPrefab, id);
                        break;
                    case Enums.ItemType.VisualEffect:
                        maxEffect = Math.Max(maxEffect, id);
                        break;
                }
            }

            if (obsolete.Count == 0)
            {
                report.AppendLine("no obsolete ammunition");
                return 0;
            }

            // D3: a converted record keeps its ID, so no ID may be in both tables.
            var clashes = obsolete.Where(item => ammunitionIds.Contains(item.Item.Id)).Select(item => item.Item.Id).ToList();
            if (clashes.Count > 0)
                throw new InvalidOperationException("IDs in both ammunition tables: " + string.Join(", ", clashes));

            var byId = obsolete.ToDictionary(item => item.Item.Id, item => item.Item);
            var converter = new AmmunitionConverter(
                Math.Max(maxAmmunition, byId.Keys.Max()) + 1, maxPrefab + 1, maxEffect + 1);

            // Convert everything before writing anything, so an error leaves the database unchanged.
            var files = new List<(string Path, string Text)>();
            foreach (var (path, item) in obsolete.OrderBy(item => item.Item.Id))
            {
                AmmunitionObsoleteSerializable coupled = null;
                if (item.CoupledAmmunitionId > 0 && !byId.TryGetValue(item.CoupledAmmunitionId, out coupled))
                    throw new InvalidOperationException($"ammunition {item.Id}: coupled record {item.CoupledAmmunitionId} not found");

                var name = Path.GetFileNameWithoutExtension(path);
                foreach (var record in converter.Convert(item, coupled, name))
                    files.Add((TargetPath(record), EditorJson.Serialize(record.Item)));
            }

            var existing = files.Where(file => File.Exists(file.Path)).Select(file => file.Path).ToList();
            if (existing.Count > 0)
                throw new InvalidOperationException("target files exist: " + string.Join(", ", existing));
            var duplicates = files.GroupBy(file => file.Path).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            if (duplicates.Count > 0)
                throw new InvalidOperationException("two records for one file: " + string.Join(", ", duplicates));

            foreach (var (path, text) in files)
            {
                File.WriteAllText(path, text, new UTF8Encoding(false));
                report.AppendLine(path);
            }

            foreach (var (path, _) in obsolete)
            {
                File.Delete(path);
                if (File.Exists(path + ".meta")) File.Delete(path + ".meta");
            }

            foreach (var folder in obsolete.Select(item => Path.GetDirectoryName(item.Path)).Distinct())
            {
                if (Directory.EnumerateFileSystemEntries(folder).Any()) continue;
                Directory.Delete(folder);
                if (File.Exists(folder + ".meta")) File.Delete(folder + ".meta");
            }

            report.AppendLine($"{obsolete.Count} obsolete records converted and deleted");
            return files.Count;
        }

        private static string TargetPath(AmmunitionConverter.NewRecord record)
        {
            switch (record.Item)
            {
                case BulletPrefabSerializable _:
                    return $"{AmmunitionFolder}/Bullets/{record.Name}.json";
                case VisualEffectSerializable _:
                    return $"{AmmunitionFolder}/Effects/{record.Name}.json";
                default:
                    return $"{AmmunitionFolder}/{record.Name}.json";
            }
        }

        private static void CopyImages(StringBuilder report)
        {
            foreach (var (from, to) in ImageCopies)
            {
                if (File.Exists(to)) continue;
                if (!AssetDatabase.CopyAsset(from, to))
                    throw new InvalidOperationException($"cannot copy {from} to {to}");
                report.AppendLine($"{from} -> {to}");
            }
        }
    }
}
