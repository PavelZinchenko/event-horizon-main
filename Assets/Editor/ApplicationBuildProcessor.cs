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
			var files = Directory.GetFiles(path, _androidManifest, SearchOption.AllDirectories);
			foreach (var filename in files)
			{
				var data = File.ReadAllText(filename);
				var result = Regex.Replace(data, "android:screenOrientation=\"\\w+\"", "android:screenOrientation=\"sensorLandscape\"");

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
