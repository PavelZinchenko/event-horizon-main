using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ReUI.Editor
{
    /// <summary>
    /// Produces a quickly testable Android APK for ReUI visual verification.
    /// It deliberately reuses the existing StreamingAssets bundles and builds
    /// ARM64 with IL2CPP while reusing existing bundles and build caches.
    /// </summary>
    public static class ReUIQuickAndroidBuild
    {
        private const string PackageName = "com.threebody.EventHorizon";
        private const string ProductName = "三体视界";
        private const string VersionName = "Beta5.1";
        // Keep the public Beta5.1 label, but advance the Android package code
        // so it can replace the earlier, incorrectly scoped Beta5.1 build.
        private const int VersionCode = 140003;
        private const string OutputFileName = "ThreeBody-EventHorizon-Beta5.1.apk";

        [MenuItem("Build/ReUI/Quick Android APK")]
        public static void Build()
        {
            ConfigureAndroidTools();
            VerifyStreamingAssets();

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            EditorUserBuildSettings.buildAppBundle = false;

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ReUIValidation.ValidateBeta5();

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are configured in EditorBuildSettings.");

            string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Android"));
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, OutputFileName);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                // The project is also mirrored for development tooling.  A clean
                // player cache prevents Unity from reusing IL2CPP artifacts whose
                // absolute source paths belong to that other checkout.
                options = BuildOptions.CleanBuildCache,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded || !File.Exists(outputPath))
            {
                throw new InvalidOperationException(
                    $"ReUI Android build failed: result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}");
            }

            var fileInfo = new FileInfo(outputPath);
            Debug.Log($"[ReUI Build] APK={outputPath}; bytes={fileInfo.Length}; result={summary.result}; duration={summary.totalTime}");
        }

        private static void VerifyStreamingAssets()
        {
            string bundlePath = Path.Combine(Application.streamingAssetsPath, "musicbundle");
            if (!File.Exists(bundlePath))
            {
                throw new FileNotFoundException(
                    "Required Android music AssetBundle is missing. Run AndroidDevelopmentBuild.Build once to generate it.",
                    bundlePath);
            }
        }

        private static void ConfigureAndroidTools()
        {
            string toolsRoot = Path.GetFullPath(Path.Combine(
                EditorApplication.applicationPath,
                "..",
                "Data",
                "PlaybackEngines",
                "AndroidPlayer"));
            string sdk = Path.Combine(toolsRoot, "SDK");
            string ndk = Path.Combine(toolsRoot, "NDK");
            string jdk = Path.Combine(toolsRoot, "OpenJDK");

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
}
