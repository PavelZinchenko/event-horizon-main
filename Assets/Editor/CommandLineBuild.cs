using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EditorWindows
{
    public static class CommandLineBuild
    {
        public static bool IsBuilding { get; private set; }

        private const string MusicBundle = "musicbundle";
        private const string MusicBackup = "Library/CommandLineBuild/StreamingAssetsBackup";
        private static readonly string[] MusicFiles =
        {
            MusicBundle, MusicBundle + ".manifest", "StreamingAssets", "StreamingAssets.manifest"
        };

        // Runs in a separate Editor process so generated code and symbols compile before Build.
        public static void Prepare()
        {
            RequireBatchMode();
            ValidateTargets();
            var target = GetTarget(Argument("-ehPlatform"));
            RequireActiveTarget(target);
            var group = BuildPipeline.GetBuildTargetGroup(target);
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            var backend = GetBackend(target);
            PlayerSettings.SetScriptingBackend(namedTarget, backend);
            ApplyDefines(namedTarget, target);
            PlayerSettings.productName = AppConfig.alternativeTitle ? "Cosmic Horizons" : "Event Horizon";

            var increment = HasArgument("-ehIncrementBuildNumber");
            if (!increment) RequireBuildNumber();
            var number = increment ? checked(AppConfig.buildNumber + 1) : AppConfig.buildNumber;
            ApplicationManager.PrepareCommandLineBuild(increment);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Argument("-ehReceipt"), number.ToString(CultureInfo.InvariantCulture));
            Debug.Log($"Prepared {target}, {backend}, build {number}.");
        }

        public static void Build()
        {
            RequireBatchMode();
            RequireBuildNumber();
            var target = GetTarget(Argument("-ehPlatform"));
            RequireActiveTarget(target);
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
            var backend = GetBackend(target);
            if (PlayerSettings.GetScriptingBackend(namedTarget) != backend)
                throw new BuildFailedException($"Preparation did not select {backend} for {target}.");

            var android = target == BuildTarget.Android;
            var outputPath = GetOutputPath(target, HasArgument("-ehAab"));
            Directory.CreateDirectory(target == BuildTarget.iOS ? outputPath : Path.GetDirectoryName(outputPath));
            if (android)
            {
                ConfigureSigning();
                EditorUserBuildSettings.buildAppBundle = HasArgument("-ehAab");
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            }

            // A persistent backup makes a killed Editor recoverable without losing local bundles.
            if (Directory.Exists(MusicBackup))
                throw new BuildFailedException($"Unrestored music files in {MusicBackup}. See docs/build.md before retrying.");
            Directory.CreateDirectory(MusicBackup);
            IsBuilding = true;
            try
            {
                HideExistingMusic();
                string manifestPath = null;
                if (IsStandalone(target)) manifestPath = BuildStandaloneMusic(target);
                AssetDatabase.Refresh();
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = GetScenes(),
                    target = target,
                    targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                    locationPathName = outputPath,
                    assetBundleManifestPath = manifestPath,
                    options = BuildOptions.CompressWithLz4HC | BuildOptions.StrictMode
                });
                if (report == null || report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"{target} player build failed. See the build log.");
                if (target == BuildTarget.StandaloneWindows64)
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(Argument("-ehReceipt")), "windows-version.txt"), AppConfig.version);
                Debug.Log($"Built {target} with build number {AppConfig.buildNumber}.");
            }
            finally
            {
                IsBuilding = false;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                RestoreMusic();
                AssetDatabase.Refresh();
            }
        }

        private static void ValidateTargets()
        {
            if (Application.unityVersion != Argument("-ehUnityVersion"))
                throw new BuildFailedException("UNITY_EDITOR does not match ProjectVersion.txt.");
            GetScenes();
            foreach (var platform in Argument("-ehTargets").Split(','))
            {
                var target = GetTarget(platform);
                if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                    throw new BuildFailedException($"Install Unity build support for {target}.");
                if (target == BuildTarget.Android) ConfigureSigning();
                else if (IsStandalone(target) && AssetDatabase.GetAssetPathsFromAssetBundle(MusicBundle).Length == 0)
                    throw new BuildFailedException("No assets are assigned to musicbundle.");
            }
            if (Directory.Exists(MusicBackup))
                throw new BuildFailedException($"Unrestored music files in {MusicBackup}. See docs/build.md.");
        }

        private static void ApplyDefines(NamedBuildTarget namedTarget, BuildTarget target)
        {
            var preserved = PlayerSettings.GetScriptingDefineSymbols(namedTarget).Split(';')
                .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
                .Where(symbol => !symbol.StartsWith("STORE_", StringComparison.Ordinal)
                    && !symbol.StartsWith("ADS_", StringComparison.Ordinal)
                    && !symbol.StartsWith("IAP_", StringComparison.Ordinal)
                    && !symbol.StartsWith("LICENSE_", StringComparison.Ordinal)
                    && symbol != "NO_GPGS" && symbol != "NO_INTERNET");
            string[] preset;
            switch (target)
            {
                case BuildTarget.Android:
                    preset = new[] { "STORE_GOOGLE", "ADS_UNITY", "IAP_UNITY", "LICENSE_COMMERCIAL" };
                    break;
                case BuildTarget.iOS:
                    preset = new[] { "STORE_APPLE", "ADS_UNITY", "IAP_UNITY", "LICENSE_COMMERCIAL", "NO_GPGS" };
                    break;
                default:
                    preset = new[] { "STORE_STEAM", "ADS_DISABLED", "IAP_DISABLED", "LICENSE_COMMERCIAL", "NO_GPGS" };
                    break;
            }
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", preserved.Concat(preset).Distinct()));
        }

        private static void ConfigureSigning()
        {
            var keystore = Environment.GetEnvironmentVariable("EH_ANDROID_KEYSTORE");
            if (string.IsNullOrEmpty(keystore)) keystore = "license.keystore";
            keystore = Path.GetFullPath(keystore);
            if (!File.Exists(keystore)) throw new BuildFailedException("Android keystore missing. Set EH_ANDROID_KEYSTORE.");
            var alias = Environment.GetEnvironmentVariable("EH_ANDROID_KEY_ALIAS");
            if (string.IsNullOrEmpty(alias)) alias = PlayerSettings.Android.keyaliasName;
            if (string.IsNullOrEmpty(alias)) throw new BuildFailedException("Set EH_ANDROID_KEY_ALIAS.");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keystorePass = RequiredEnvironment("EH_ANDROID_KEYSTORE_PASSWORD");
            PlayerSettings.Android.keyaliasPass = RequiredEnvironment("EH_ANDROID_KEY_PASSWORD");
        }

        private static string RequiredEnvironment(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value)) throw new BuildFailedException($"Set {name} before building Android.");
            return value;
        }

        private static string BuildStandaloneMusic(BuildTarget target)
        {
            var cache = "Library/CommandLineBuild/" + target + "Music";
            Directory.CreateDirectory(cache);
            var assets = AssetDatabase.GetAssetPathsFromAssetBundle(MusicBundle);
            if (assets.Length == 0) throw new BuildFailedException("No assets are assigned to musicbundle.");
            var manifest = BuildPipeline.BuildAssetBundles(cache,
                new[] { new AssetBundleBuild { assetBundleName = MusicBundle, assetNames = assets } },
                BuildAssetBundleOptions.None, target);
            if (manifest == null || !manifest.GetAllAssetBundles().Contains(MusicBundle))
                throw new BuildFailedException($"{target} music bundle build failed.");
            Directory.CreateDirectory(Application.streamingAssetsPath);
            foreach (var file in new[] { MusicBundle, MusicBundle + ".manifest" })
            {
                // Record new files before copying so cleanup also handles partial copies.
                File.AppendAllLines(Path.Combine(MusicBackup, "created.txt"), new[] { file });
                File.Copy(Path.Combine(cache, file), Path.Combine(Application.streamingAssetsPath, file));
            }
            return cache + "/" + Path.GetFileName(cache) + ".manifest";
        }

        private static void HideExistingMusic()
        {
            foreach (var name in MusicFiles.SelectMany(file => new[] { file, file + ".meta" }))
            {
                var source = Path.Combine(Application.streamingAssetsPath, name);
                if (File.Exists(source)) File.Move(source, Path.Combine(MusicBackup, name));
            }
        }

        private static void RestoreMusic()
        {
            var createdList = Path.Combine(MusicBackup, "created.txt");
            if (File.Exists(createdList))
            {
                foreach (var file in File.ReadAllLines(createdList))
                {
                    File.Delete(Path.Combine(Application.streamingAssetsPath, file));
                    File.Delete(Path.Combine(Application.streamingAssetsPath, file + ".meta"));
                }
                File.Delete(createdList);
            }
            foreach (var file in Directory.GetFiles(MusicBackup))
                File.Move(file, Path.Combine(Application.streamingAssetsPath, Path.GetFileName(file)));
            Directory.Delete(MusicBackup); // Only remove an empty backup; never discard unrecovered files.
        }

        private static string[] GetScenes()
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0 || scenes.Any(scene => !File.Exists(scene)))
                throw new BuildFailedException("Build Settings must contain enabled scenes that exist on disk.");
            return scenes;
        }

        private static BuildTarget GetTarget(string platform)
        {
            switch (platform)
            {
                case "android": return BuildTarget.Android;
                case "windows": return BuildTarget.StandaloneWindows64;
                case "linux": return BuildTarget.StandaloneLinux64;
                case "mac": return BuildTarget.StandaloneOSX;
                case "ios": return BuildTarget.iOS;
                default: throw new BuildFailedException($"Unsupported build platform: {platform}");
            }
        }

        private static ScriptingImplementation GetBackend(BuildTarget target)
        {
            return target == BuildTarget.StandaloneOSX
                ? ScriptingImplementation.Mono2x
                : ScriptingImplementation.IL2CPP;
        }

        private static bool IsStandalone(BuildTarget target)
        {
            return BuildPipeline.GetBuildTargetGroup(target) == BuildTargetGroup.Standalone;
        }

        private static string GetOutputPath(BuildTarget target, bool appBundle)
        {
            switch (target)
            {
                case BuildTarget.Android: return "Builds/Android/eventhorizon" + (appBundle ? ".aab" : ".apk");
                case BuildTarget.StandaloneWindows64: return "Builds/Windows/eventhorizon.exe";
                case BuildTarget.StandaloneLinux64: return "Builds/Linux/eventhorizon.x86_64";
                case BuildTarget.StandaloneOSX: return "Builds/Mac/eventhorizon.app";
                case BuildTarget.iOS: return "Builds/iOS";
                default: throw new BuildFailedException($"Unsupported build target: {target}");
            }
        }

        private static void RequireActiveTarget(BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
                throw new BuildFailedException($"Launch Unity with -buildTarget for {target}.");
        }

        private static void RequireBuildNumber()
        {
            if (!int.TryParse(Argument("-ehBuildNumber"), out var number) || number != AppConfig.buildNumber)
                throw new BuildFailedException("Compiled AppConfig does not contain the requested build number. Run preparation first.");
        }

        private static void RequireBatchMode()
        {
            if (!Application.isBatchMode) throw new BuildFailedException("Use the scripts in Builds/ to run this method.");
        }

        private static bool HasArgument(string name) => Environment.GetCommandLineArgs().Contains(name);

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, name);
            if (index < 0 || index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
                throw new BuildFailedException($"Missing argument: {name}");
            return args[index + 1];
        }
    }
}
