using UnityEditor;
using UnityEngine;
using System;

public class CodeGenerator : EditorWindow
{
	[MenuItem("Window/Code Generator")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(CodeGenerator));
	}
	
	private void OnGUI()
	{
		_deviceId = EditorGUILayout.TextField("Device ID", _deviceId);

		if (string.IsNullOrEmpty(_deviceId))
			return;

		var hash = Convert.ToInt32(_deviceId);
		for (int i = 0; i < 20; ++i)
		{
			var code = Security.DebugCommands.Encode(i, hash);
			GUILayout.Label(i + ": " + code + " - " + Security.DebugCommands.Decode(code, hash));
		}
	}

	private string _deviceId;
}
