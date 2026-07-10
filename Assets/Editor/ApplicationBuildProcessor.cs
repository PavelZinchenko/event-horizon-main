using System.IO;
using UnityEditor.Android;
using System.Text.RegularExpressions;

public class ApplicationBuildProcessor : IPostGenerateGradleAndroidProject
{
	public int callbackOrder => 0;

	public void OnPostGenerateGradleAndroidProject(string path)
	{
		try
		{
			var gradleRoot = Directory.GetParent(path)?.FullName ?? path;
			var sdk = System.Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
			var ndk = System.Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT");
			if (!string.IsNullOrEmpty(sdk))
			{
				var properties = "sdk.dir=" + sdk.Replace('\\', '/') + System.Environment.NewLine;
				if (!string.IsNullOrEmpty(ndk))
					properties += "ndk.dir=" + ndk.Replace('\\', '/') + System.Environment.NewLine;
				File.WriteAllText(Path.Combine(gradleRoot, "local.properties"), properties);
			}

			var files = Directory.GetFiles(path, _androidManifest, SearchOption.AllDirectories);
			foreach (var filename in files)
			{
				var data = File.ReadAllText(filename);
				var result = Regex.Replace(data, "android:screenOrientation=\"\\w+\"", "android:screenOrientation=\"sensorLandscape\"");

				// Keep Unity's generated launcher/activity manifest intact and add
				// only the legacy shared-storage declarations needed by Android 12
				// and earlier.  Replacing the entire manifest removed Unity's launch
				// activity and produced an APK that could not be started.
				if (!result.Contains("android.permission.READ_EXTERNAL_STORAGE"))
				{
					const string permissions =
						"\n  <uses-permission android:name=\"android.permission.READ_EXTERNAL_STORAGE\" android:maxSdkVersion=\"32\" />" +
						"\n  <uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\" android:maxSdkVersion=\"28\" />";
					result = Regex.Replace(result, "(<manifest\\b[^>]*>)", "$1" + permissions, RegexOptions.IgnoreCase);
				}

				if (!result.Contains("android:requestLegacyExternalStorage="))
					result = Regex.Replace(result, "<application\\b", "<application android:requestLegacyExternalStorage=\"true\"", RegexOptions.IgnoreCase);

				if (data != result)
					File.WriteAllText(filename, result);
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogException(e);
		}
	}

	const string _androidManifest = "AndroidManifest.xml";
}
