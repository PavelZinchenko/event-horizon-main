using System;
using System.IO;
using Combat.Integration.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Phase 7, step 9: writes the combat source hash (CoreSourceHash) into a Resources text file before every build, from
// the Build window and from the command line alike. The file is git-ignored: it changes with every source change.
public sealed class CoreSourceHashBuildStep : IPreprocessBuildWithReport
{
    public const string AssetPath = "Assets/Resources/" + CoreSourceHash.ResourceName + ".txt";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var text = Convert.ToBase64String(CoreSourceHash.Compute(Path.GetDirectoryName(Application.dataPath)));
        if (!File.Exists(AssetPath) || File.ReadAllText(AssetPath).Trim() != text)
        {
            File.WriteAllText(AssetPath, text);
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);
        }
        Debug.Log($"CoreSourceHashBuildStep: combat source hash {text}");
    }
}
