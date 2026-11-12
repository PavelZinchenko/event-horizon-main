using Combat.Integration.Core;
using UnityEditor;
using UnityEngine;

// Migration phase 5b, step 3: plays the new combat effects and sounds in a running core battle, so the owner can review
// each one before it has users. Start a battle with the new simulator, then press a button.
public class CombatEffectPreviewWindow : EditorWindow
{
    [MenuItem("Combat/Effect Preview")]
    private static void Open() => GetWindow<CombatEffectPreviewWindow>("Effect Preview");

    private void OnInspectorUpdate() => Repaint();

    private void OnGUI()
    {
        var preview = CoreEffectPreview.Current;
        if (!Application.isPlaying || preview == null)
        {
            EditorGUILayout.HelpBox("Start a battle with the new combat simulator.", MessageType.Info);
            return;
        }

        if (!preview.HasShip)
            EditorGUILayout.HelpBox("No player ship: effects need one to play in front of.", MessageType.Warning);

        preview.Size = EditorGUILayout.FloatField("Size", preview.Size);
        preview.Rotation = EditorGUILayout.FloatField("Rotation", preview.Rotation);
        preview.Distance = EditorGUILayout.FloatField("Distance ahead", preview.Distance);
        preview.Seconds = EditorGUILayout.FloatField(new GUIContent("Seconds", "0: a record's own lifetime, 1 s for a prefab"), preview.Seconds);
        preview.OnShip = EditorGUILayout.Toggle(new GUIContent("On the ship", "Attached to the ship view, so it follows it"), preview.OnShip);
        using (new EditorGUILayout.HorizontalScope())
        {
            preview.SetColor = EditorGUILayout.Toggle(new GUIContent("Owner colour", "The colour the element colour modes apply"), preview.SetColor);
            using (new EditorGUI.DisabledScope(!preview.SetColor))
                preview.Color = EditorGUILayout.ColorField(preview.Color);
        }

        _tab = GUILayout.Toolbar(_tab, Tabs);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        switch (_tab)
        {
            case 0: DrawItems(preview, preview.Types); break;
            case 1: DrawItems(preview, preview.Records); break;
            case 2: DrawItems(preview, preview.Prefabs); break;
            case 3: DrawSounds(preview); break;
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawItems(CoreEffectPreview preview, System.Collections.Generic.List<CoreEffectPreview.Item> items)
    {
        foreach (var item in items)
            using (new EditorGUI.DisabledScope(item.IsEmpty))
                if (GUILayout.Button(item.Name, EditorStyles.miniButton))
                    preview.Play(item);
    }

    private static void DrawSounds(CoreEffectPreview preview)
    {
        if (GUILayout.Button("Stop loops"))
            preview.StopSounds();

        foreach (var name in preview.Sounds)
            if (GUILayout.Button(name, EditorStyles.miniButton))
                preview.PlaySound(name);
    }

    private static readonly string[] Tabs = { "Types", "Records", "Prefabs", "Sounds" };
    private int _tab;
    private Vector2 _scroll;
}
