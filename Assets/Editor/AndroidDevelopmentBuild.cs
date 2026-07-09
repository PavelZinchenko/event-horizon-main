using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class AndroidDevelopmentBuild
{
    private const string PackageName = "com.threebody.EventHorizon";
    private const string ProductName = "三体视界";
    private const string VersionName = "Preview 11";
    private const int VersionCode = 112123;

    [MenuItem("Build/Android/Development APK")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        ConfigureAndroidTools();
        RefreshResourceLocator();
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
        PlayerSettings.productName = ProductName;
        PlayerSettings.bundleVersion = VersionName;
        PlayerSettings.Android.bundleVersionCode = VersionCode;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        var outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Android"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "ThreeBody-EventHorizon-Preview-11.apk");
        BuildStreamingAssetBundles();

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes are configured in EditorBuildSettings.");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Android build failed: {report.summary.result}, errors: {report.summary.totalErrors}");

        Debug.Log($"Android development APK: {outputPath}");
    }

    private static void BuildStreamingAssetBundles()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        var manifest = BuildPipeline.BuildAssetBundles(
            Application.streamingAssetsPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.Android);

        if (manifest == null || !manifest.GetAllAssetBundles().Contains("musicbundle"))
            throw new InvalidOperationException("Android music AssetBundle was not built.");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void RefreshResourceLocator()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        NormalizeShipSpriteScale("Assets/Sprites/Ships");
        NormalizeShipSpriteScale("Assets/Sprites/ShipIcons");
        NormalizeShipSpriteScale("Assets/Sprites/Starbases");
        NormalizeThreeBodyComponentSprites();
        var locator = Resources.Load<Services.Resources.ResourceLocator>("ResourceLocator");
        if (locator == null)
            throw new InvalidOperationException("Resources/ResourceLocator prefab was not found.");

        locator.Reload();
        AssetDatabase.SaveAssets();
    }

    private static void NormalizeShipSpriteScale(string folder)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                continue;

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var pixelsPerUnit = Mathf.Max(width, height);
            if (pixelsPerUnit <= 0)
                continue;

            var changed = importer.textureType != TextureImporterType.Sprite ||
                          importer.spriteImportMode != SpriteImportMode.Single ||
                          !importer.alphaIsTransparency ||
                          importer.mipmapEnabled ||
                          !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit);
            if (!changed)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.SaveAndReimport();
        }
    }

    private static void NormalizeThreeBodyComponentSprites()
    {
        var databaseFolder = Path.Combine(Application.dataPath, "Modules", "Database", "Resources", "Database", "Component");
        foreach (var jsonPath in Directory.GetFiles(databaseFolder, "*.json", SearchOption.TopDirectoryOnly))
        {
            var component = JsonUtility.FromJson<ComponentSpriteMetadata>(File.ReadAllText(jsonPath));
            if (component == null || component.ContentSource != 1 || string.IsNullOrEmpty(component.Icon) || string.IsNullOrEmpty(component.Layout))
                continue;

            var icon = component.Icon.EndsWith("_0", StringComparison.OrdinalIgnoreCase)
                ? component.Icon.Substring(0, component.Icon.Length - 2)
                : component.Icon;
            var assetPath = FindComponentSpriteAsset(icon);
            if (string.IsNullOrEmpty(assetPath) || AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
                throw new InvalidOperationException($"ThreeBody component sprite is missing: {component.Icon} ({jsonPath})");

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var gridSize = Mathf.CeilToInt(Mathf.Sqrt(component.Layout.Length));
            var pixelsPerUnit = Mathf.Max(width, height) / (float)Mathf.Max(gridSize, 1);
            var changed = importer.textureType != TextureImporterType.Sprite ||
                          importer.spriteImportMode != SpriteImportMode.Single ||
                          !importer.alphaIsTransparency ||
                          importer.mipmapEnabled ||
                          !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit);
            if (!changed)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.SaveAndReimport();
        }
    }

    private static string FindComponentSpriteAsset(string icon)
    {
        var folder = "Assets/Sprites/Components/";
        foreach (var extension in new[] { ".png", ".jpg", ".jpeg", ".JPG", ".psd" })
        {
            var path = folder + icon + extension;
            if (File.Exists(Path.GetFullPath(path)))
                return path;
        }

        return null;
    }

    [Serializable]
    private sealed class ComponentSpriteMetadata
    {
        public int ContentSource;
        public string Icon;
        public string Layout;
    }

    private static void ConfigureAndroidTools()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var toolsRoot = Path.Combine(localAppData, "Unity", "AndroidPlayer");
        var sdk = Path.Combine(toolsRoot, "SDK");
        var ndk = Path.Combine(sdk, "ndk", "27.2.12479018");
        var jdk = Path.Combine(toolsRoot, "OpenJDK");

        if (!Directory.Exists(sdk) || !Directory.Exists(ndk) || !Directory.Exists(jdk))
        {
            toolsRoot = Path.GetFullPath(Path.Combine(EditorApplication.applicationPath, "..", "Data", "PlaybackEngines", "AndroidPlayer"));
            sdk = Path.Combine(toolsRoot, "SDK");
            ndk = Path.Combine(toolsRoot, "NDK");
            jdk = Path.Combine(toolsRoot, "OpenJDK");
        }

        if (!Directory.Exists(sdk) || !Directory.Exists(ndk) || !Directory.Exists(jdk))
            throw new DirectoryNotFoundException($"Android tools are incomplete under {toolsRoot}");

        Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdk);
        Environment.SetEnvironmentVariable("ANDROID_HOME", sdk);
        Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndk);
        Environment.SetEnvironmentVariable("JAVA_HOME", jdk);

        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);
        EditorPrefs.SetString("AndroidNdkRootR16b", ndk);
        EditorPrefs.SetString("JdkPath", jdk);
    }
}
