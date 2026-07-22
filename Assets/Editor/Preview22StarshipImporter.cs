#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class Preview22StarshipImporter
{
    private const string ArchivePath = "Assets/Editor/Preview22/StarshipEarth.zip";
    private const string DatabaseRoot = "Assets/Modules/Database/Resources/Database/Ship";

    [MenuItem("Tools/三体视界/导入 Preview22 星舰地球")]
    public static void Import()
    {
        var archive = Path.GetFullPath(ArchivePath);
        var temporary = Path.Combine(Path.GetTempPath(), "ThreeBodyPreview22Starship");
        if (Directory.Exists(temporary)) Directory.Delete(temporary, true);
        ZipFile.ExtractToDirectory(archive, temporary);

        var root = Directory.GetDirectories(temporary)[0];
        var mapping = new Dictionary<string, string>
        {
            ["E护卫"] = "StarshipEarthFrigate",
            ["E驱逐"] = "StarshipEarthDestroyer",
            ["E巡洋"] = "StarshipEarthCruiser",
            ["E战列"] = "StarshipEarthBattleship",
            ["E旗舰"] = "StarshipEarthFlagship",
            ["星舰地球空间站"] = "StarshipEarthStation"
        };

        foreach (var pair in mapping)
        {
            var source = JObject.Parse(File.ReadAllText(Path.Combine(root, pair.Key + ".json")));
            var targetPath = DatabaseRoot + "/" + pair.Value + ".json";
            var target = JObject.Parse(File.ReadAllText(targetPath));
            foreach (var property in new[] { "IconScale", "ModelScale", "EngineColor", "Engines", "Layout", "Barrels", "Features" })
                if (source[property] != null) target[property] = source[property].DeepClone();
            File.WriteAllText(targetPath, target.ToString());

            var png = File.ReadAllBytes(Path.Combine(root, pair.Key + ".png"));
            File.WriteAllBytes("Assets/Sprites/Ships/" + target["ModelImage"] + ".png", png);
            File.WriteAllBytes("Assets/Sprites/ShipIcons/" + target["IconImage"] + ".png", png);
        }

        Directory.Delete(temporary, true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("Preview22 Starship Earth layouts and sprites imported.");
    }
}
#endif
