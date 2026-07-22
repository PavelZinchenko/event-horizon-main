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

				// Keep Unity's generated launcher/activity manifest intact.  Add each
				// storage declaration independently: a manifest merger may already
				// contain the legacy permission while still lacking Android 13's
				// READ_MEDIA_* permissions needed by the system picker.
				var permissions = string.Empty;
				if (!result.Contains("android.permission.READ_EXTERNAL_STORAGE"))
					permissions += "\n  <uses-permission android:name=\"android.permission.READ_EXTERNAL_STORAGE\" android:maxSdkVersion=\"32\" />";
				if (!result.Contains("android.permission.WRITE_EXTERNAL_STORAGE"))
					permissions += "\n  <uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\" android:maxSdkVersion=\"28\" />";
				if (!result.Contains("android.permission.READ_MEDIA_IMAGES"))
					permissions += "\n  <uses-permission android:name=\"android.permission.READ_MEDIA_IMAGES\" />";
				if (!result.Contains("android.permission.READ_MEDIA_VIDEO"))
					permissions += "\n  <uses-permission android:name=\"android.permission.READ_MEDIA_VIDEO\" />";
				if (!result.Contains("android.permission.READ_MEDIA_AUDIO"))
					permissions += "\n  <uses-permission android:name=\"android.permission.READ_MEDIA_AUDIO\" />";
				if (!string.IsNullOrEmpty(permissions))
					result = Regex.Replace(result, "(<manifest\\b[^>]*>)", "$1" + permissions, RegexOptions.IgnoreCase);

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
