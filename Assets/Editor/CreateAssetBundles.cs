using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
  	    if (!Directory.Exists(Application.streamingAssetsPath))
    	    Directory.CreateDirectory(_assetBundleDirectory);

  	    BuildPipeline.BuildAssetBundles(
            _assetBundleDirectory, 
            BuildAssetBundleOptions.None, 
            EditorUserBuildSettings.activeBuildTarget);
	}

    private const string _assetBundleDirectory = "Assets/StreamingAssets";
}
