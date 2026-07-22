using UnityEditor;
using UnityEditor.UI;

namespace Gui.Theme.Wrappers
{
    [CustomEditor(typeof(ThemedText))]
    [CanEditMultipleObjects]
    public class ThemedTextEditor : TextEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_themeColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_colorMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_themeFont"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_themeFontSize"));
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
