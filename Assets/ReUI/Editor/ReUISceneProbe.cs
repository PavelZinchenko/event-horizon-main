using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReUI.Editor
{
    public static class ReUISceneProbe
    {
        public static void DumpTargetScenes()
        {
            DumpScene("Assets/Scenes/MainMenuScene.unity");
            DumpScene("Assets/Scenes/StarMapScene.unity");
            DumpScene("Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity");
        }

        public static void DumpTargetSubtrees()
        {
            Scene starMap = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            DumpSubtree(starMap, "Canvas/GameMenu");
            DumpSubtree(starMap, "Canvas/StatusPanel");

            Scene shipEditor = EditorSceneManager.OpenScene(
                "Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity", OpenSceneMode.Single);
            DumpSubtree(shipEditor, "Canvas/ShipEditorWindow/Buttons");
            DumpSubtree(shipEditor, "Canvas/ShipEditorWindow/RightPanel");
        }

        public static void DumpCombatAndResearchTargets()
        {
            Scene combat = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            DumpSubtree(combat, "Canvas/LeftStatsPanel");
            DumpSubtree(combat, "Canvas/RightStatsPanel");

            Scene starMap = EditorSceneManager.OpenScene("Assets/Scenes/StarMapScene.unity", OpenSceneMode.Single);
            DumpSubtree(starMap, "Canvas/Panels/ResearchPanel");
        }

        public static void DumpCombatScene()
        {
            DumpScene("Assets/Scenes/CombatScene.unity");
            Scene combat = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            DumpSubtree(combat, "Canvas/Player");
            DumpSubtree(combat, "Canvas/Enemy");
            DumpSubtree(combat, "Canvas/PausedMenu");
            DumpSubtree(combat, "Canvas/PauseMenu");
            DumpSubtree(combat, "Canvas/SettingsPanel");
        }

        public static void DumpCombatTexts()
        {
            Scene combat = EditorSceneManager.OpenScene("Assets/Scenes/CombatScene.unity", OpenSceneMode.Single);
            var output = new StringBuilder();
            output.AppendLine("[ReUI Combat Text Probe]");
            foreach (GameObject root in combat.GetRootGameObjects())
            {
                Text[] texts = root.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    RectTransform rect = texts[i].rectTransform;
                    output.Append("TEXT path=").Append(GetPath(texts[i].transform))
                        .Append(" value='").Append((texts[i].text ?? string.Empty).Replace('\n', ' ')).Append("'")
                        .Append(" color=").Append(texts[i].color)
                        .Append(" pos=").Append(rect.anchoredPosition)
                        .Append(" size=").Append(rect.sizeDelta)
                        .AppendLine();
                }
            }
            Debug.Log(output.ToString());
        }

        public static void DumpResearchPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents("Assets/Resources/Gui/StarMapScene/ResearchPanel.prefab");
            try
            {
                var output = new StringBuilder();
                output.AppendLine("[ReUI Research Probe] PREFAB=ResearchPanel");
                Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < nodes.Length; i++)
                {
                    Transform node = nodes[i];
                    Image image = node.GetComponent<Image>();
                    Button button = node.GetComponent<Button>();
                    Toggle toggle = node.GetComponent<Toggle>();
                    if (image == null && button == null && toggle == null) continue;
                    output.Append("NODE path=").Append(GetPath(node, root.transform.parent));
                    if (image != null)
                        output.Append(" image=").Append(image.sprite == null ? "null" : image.sprite.name)
                            .Append(':').Append(image.enabled ? "on" : "off");
                    if (button != null)
                        output.Append(" button target=").Append(button.targetGraphic == null ? "null" : button.targetGraphic.name);
                    if (toggle != null)
                    {
                        output.Append(" toggle interactable=").Append(toggle.interactable)
                            .Append(" group=").Append(toggle.group == null ? "null" : toggle.group.name)
                            .Append(" onValueMethods=");
                        for (int e = 0; e < toggle.onValueChanged.GetPersistentEventCount(); e++)
                        {
                            if (e > 0) output.Append(',');
                            output.Append(toggle.onValueChanged.GetPersistentMethodName(e));
                        }
                    }
                    output.AppendLine();
                }
                Debug.Log(output.ToString());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static void DumpReUI7Market()
        {
            DumpFilteredPrefabButtons(
                "Assets/Resources/Gui/StarMapScene/MarketDialog.prefab",
                new[] { "" });
            DumpPrefabToggles("Assets/Resources/Gui/StarMapScene/MarketDialog.prefab");
        }

        private static void DumpPrefabToggles(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var output = new StringBuilder();
                output.AppendLine($"[ReUI7 Market Toggle Probe] PREFAB={path}");
                Toggle[] toggles = root.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    Toggle toggle = toggles[i];
                    Text label = toggle.GetComponentInChildren<Text>(true);
                    Image target = toggle.targetGraphic as Image;
                    output.Append("TOGGLE path=").Append(GetPath(toggle.transform, root.transform.parent))
                        .Append(" isOn=").Append(toggle.isOn)
                        .Append(" label='").Append(label == null ? string.Empty : (label.text ?? string.Empty).Replace('\n', ' ')).Append("'")
                        .Append(" target=").Append(target == null ? "null" : GetPath(target.transform, root.transform.parent))
                        .Append(" targetSprite=").Append(target == null || target.sprite == null ? "null" : target.sprite.name)
                        .Append(" targetColor=").Append(target == null ? "null" : target.color.ToString())
                        .Append(" graphic=").Append(toggle.graphic == null ? "null" : GetPath(toggle.graphic.transform, root.transform.parent))
                        .Append(" images=");
                    Image[] images = toggle.GetComponentsInChildren<Image>(true);
                    for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
                    {
                        if (imageIndex > 0) output.Append('|');
                        output.Append(GetPath(images[imageIndex].transform, toggle.transform))
                            .Append(':').Append(images[imageIndex].sprite == null ? "null" : images[imageIndex].sprite.name)
                            .Append(':').Append(images[imageIndex].color)
                            .Append(':').Append(images[imageIndex].enabled ? "on" : "off");
                    }
                    output.AppendLine();
                }
                Debug.Log(output.ToString());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static void DumpReUI7Toggles()
        {
            DumpSceneToggles("Assets/Scenes/SettingsScene.unity");
            DumpSceneToggles("Assets/Scenes/StarMapScene.unity");
        }

        private static void DumpSceneToggles(string path)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var output = new StringBuilder();
            output.AppendLine($"[ReUI7 Toggle Probe] SCENE={scene.name}");
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Toggle[] toggles = root.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    Toggle toggle = toggles[i];
                    RectTransform rect = toggle.transform as RectTransform;
                    Text label = toggle.GetComponentInChildren<Text>(true);
                    Image target = toggle.targetGraphic as Image;
                    output.Append("TOGGLE path=").Append(GetPath(toggle.transform))
                        .Append(" active=").Append(toggle.gameObject.activeInHierarchy)
                        .Append(" isOn=").Append(toggle.isOn)
                        .Append(" label='").Append(label == null ? string.Empty : (label.text ?? string.Empty).Replace('\n', ' ')).Append("'")
                        .Append(" rect=").Append(rect == null ? "null" : rect.rect.width.ToString("0") + "x" + rect.rect.height.ToString("0"))
                        .Append(" target=").Append(target == null ? "null" : GetPath(target.transform))
                        .Append(" targetSprite=").Append(target == null || target.sprite == null ? "null" : target.sprite.name)
                        .Append(" graphic=").Append(toggle.graphic == null ? "null" : GetPath(toggle.graphic.transform))
                        .Append(" methods=");
                    for (int e = 0; e < toggle.onValueChanged.GetPersistentEventCount(); e++)
                    {
                        if (e > 0) output.Append(',');
                        output.Append(toggle.onValueChanged.GetPersistentMethodName(e));
                    }
                    output.Append(" images=");
                    Image[] images = toggle.GetComponentsInChildren<Image>(true);
                    for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
                    {
                        if (imageIndex > 0) output.Append('|');
                        output.Append(GetPath(images[imageIndex].transform, toggle.transform))
                            .Append(':').Append(images[imageIndex].sprite == null ? "null" : images[imageIndex].sprite.name)
                            .Append(':').Append(images[imageIndex].color)
                            .Append(':').Append(images[imageIndex].enabled ? "on" : "off");
                    }
                    output.AppendLine();
                }
            }
            Debug.Log(output.ToString());
        }

        public static void DumpReUI7Targets()
        {
            DumpFilteredSceneButtons(
                "Assets/Scenes/SettingsScene.unity",
                new[] { "Settings", "Exit", "Close", "General", "Combat", "Control", "Graphics", "Database", "Map" });
            DumpFilteredSceneButtons(
                "Assets/Scenes/StarMapScene.unity",
                new[] { "GameMenu", "Shop", "Store", "Fleet", "Skills", "Research", "CargoHold", "Exit", "Arena", "Fight", "Preview" });
            DumpFilteredPrefabButtons(
                "Assets/Resources/Prefabs/Gui/SettingsPanel.prefab",
                new[] { "Settings", "Exit", "Close", "General", "Combat", "Control", "Graphics", "Database", "Map" });
            DumpFilteredPrefabButtons(
                "Assets/Resources/Gui/StarMapScene/ArenaFight.prefab",
                new[] { "Fight", "Cancel", "Enemy", "Buttons" });
            DumpFilteredPrefabButtons(
                "Assets/Resources/Gui/StarMapScene/SpecialStoreDialog.prefab",
                new[] { "Buy", "Purchase", "Exit", "Close", "Shop", "Store", "Tab" });
            DumpFilteredPrefabButtons(
                "Assets/Resources/Gui/StarMapScene/IapStoreDialog.prefab",
                new[] { "Buy", "Purchase", "Exit", "Close", "Shop", "Store", "Tab" });
        }

        private static void DumpFilteredSceneButtons(string path, string[] filters)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var output = new StringBuilder();
            output.AppendLine($"[ReUI7 Probe] SCENE={scene.name}");
            foreach (GameObject root in scene.GetRootGameObjects())
                AppendFilteredButtons(output, root.transform, filters);
            Debug.Log(output.ToString());
        }

        private static void DumpFilteredPrefabButtons(string path, string[] filters)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var output = new StringBuilder();
                output.AppendLine($"[ReUI7 Probe] PREFAB={path}");
                AppendFilteredButtons(output, root.transform, filters);
                Debug.Log(output.ToString());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AppendFilteredButtons(StringBuilder output, Transform root, string[] filters)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                string path = GetPath(button.transform);
                Text label = button.GetComponentInChildren<Text>(true);
                string labelValue = label == null ? string.Empty : (label.text ?? string.Empty).Replace('\n', ' ');
                string combined = (path + " " + labelValue).ToLowerInvariant();
                bool match = false;
                for (int f = 0; f < filters.Length; f++)
                {
                    if (combined.Contains(filters[f].ToLowerInvariant()))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match) continue;

                RectTransform rect = button.transform as RectTransform;
                Image target = button.targetGraphic as Image;
                output.Append("BUTTON path=").Append(path)
                    .Append(" active=").Append(button.gameObject.activeInHierarchy)
                    .Append(" interactable=").Append(button.interactable)
                    .Append(" label='").Append(labelValue).Append("'")
                    .Append(" rect=").Append(rect == null ? "null" : rect.rect.width.ToString("0") + "x" + rect.rect.height.ToString("0"))
                    .Append(" target=").Append(target == null ? "null" : GetPath(target.transform))
                    .Append(" targetSprite=").Append(target == null || target.sprite == null ? "null" : target.sprite.name)
                    .Append(" targetColor=").Append(target == null ? "null" : target.color.ToString())
                    .Append(" methods=");
                for (int e = 0; e < button.onClick.GetPersistentEventCount(); e++)
                {
                    if (e > 0) output.Append(',');
                    output.Append(button.onClick.GetPersistentMethodName(e));
                }
                output.Append(" images=");
                Image[] images = button.GetComponentsInChildren<Image>(true);
                for (int imageIndex = 0; imageIndex < images.Length; imageIndex++)
                {
                    if (imageIndex > 0) output.Append('|');
                    output.Append(GetPath(images[imageIndex].transform, button.transform))
                        .Append(':').Append(images[imageIndex].sprite == null ? "null" : images[imageIndex].sprite.name)
                        .Append(':').Append(images[imageIndex].color)
                        .Append(':').Append(images[imageIndex].enabled ? "on" : "off");
                }
                output.AppendLine();
            }
        }

        public static void DumpShipHudTargets()
        {
            DumpSceneControls("Assets/Scenes/CombatScene.unity");
            DumpSceneControls("Assets/Scenes/StarMapScene.unity");
            DumpSceneControls("Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity");
            DumpPrefabControls("Assets/Resources/Prefabs/Gui/PlayerShipItem.prefab");
            DumpPrefabControls("Assets/Resources/Prefabs/Gui/EnemyShipItem.prefab");
            DumpPrefabControls("Assets/Resources/Prefabs/Gui/ShipToggleButton.prefab");
            DumpPrefabControls("Assets/Resources/Prefabs/Gui/ShipInHangar.prefab");
        }

        private static void DumpSceneControls(string path)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var output = new StringBuilder();
            output.AppendLine($"[ReUI HUD Probe] SCENE={scene.name}");
            foreach (GameObject root in scene.GetRootGameObjects())
                AppendControls(output, root.transform);
            Debug.Log(output.ToString());
        }

        private static void DumpPrefabControls(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var output = new StringBuilder();
                output.AppendLine($"[ReUI HUD Probe] PREFAB={path}");
                AppendControls(output, root.transform);
                Debug.Log(output.ToString());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AppendControls(StringBuilder output, Transform root)
        {
            foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
            {
                Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
                Image background = slider.GetComponent<Image>();
                output.Append("SLIDER path=").Append(GetPath(slider.transform))
                    .Append(" direction=").Append(slider.direction)
                    .Append(" fill=").Append(fill == null ? "null" : GetPath(fill.transform))
                    .Append(" fillSprite=").Append(fill == null || fill.sprite == null ? "null" : fill.sprite.name)
                    .Append(" fillType=").Append(fill == null ? "null" : fill.type.ToString())
                    .Append(" backgroundSprite=").Append(background == null || background.sprite == null ? "null" : background.sprite.name)
                    .AppendLine();
            }

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                string imagePath = GetPath(image.transform);
                string lowerPath = imagePath.ToLowerInvariant();
                string spriteName = image.sprite == null ? string.Empty : image.sprite.name;
                if (!(lowerPath.Contains("health") || lowerPath.Contains("energy") || lowerPath.Contains("hitpoint") ||
                      lowerPath.Contains("armor") || lowerPath.Contains("shield") || lowerPath.Contains("condition") ||
                      spriteName.Contains("bar") || spriteName.Contains("tile") || image.type == Image.Type.Filled))
                    continue;

                output.Append("IMAGE path=").Append(imagePath)
                    .Append(" sprite=").Append(string.IsNullOrEmpty(spriteName) ? "null" : spriteName)
                    .Append(" type=").Append(image.type)
                    .Append(" fillMethod=").Append(image.fillMethod)
                    .Append(" fillAmount=").Append(image.fillAmount.ToString("0.000"))
                    .Append(" color=").Append(image.color)
                    .AppendLine();
            }

            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                string lower = text.name.ToLowerInvariant();
                string parent = text.transform.parent == null ? string.Empty : text.transform.parent.name.ToLowerInvariant();
                if (!(lower.Contains("level") || lower.Contains("rank") || lower.Contains("class") ||
                      lower.Contains("value") || lower.Contains("health") || lower.Contains("energy") ||
                      parent.Contains("stats") || parent.Contains("ship")))
                    continue;

                output.Append("TEXT path=").Append(GetPath(text.transform))
                    .Append(" value='").Append((text.text ?? string.Empty).Replace('\n', ' ')).Append("'")
                    .Append(" color=").Append(text.color)
                    .AppendLine();
            }
        }

        private static void DumpScene(string path)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var output = new StringBuilder();
            output.AppendLine($"[ReUI Probe] SCENE={scene.name}");

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    Text text = button.GetComponentInChildren<Text>(true);
                    string label = text == null ? "" : text.text.Replace('\n', ' ');
                    string target = button.targetGraphic == null ? "null" : button.targetGraphic.name;
                    string sprite = button.targetGraphic is Image image && image.sprite != null ? image.sprite.name : "null";
                    string methods = "";
                    int count = button.onClick.GetPersistentEventCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (i > 0) methods += ",";
                        methods += button.onClick.GetPersistentMethodName(i);
                    }

                    output.Append("BUTTON path=").Append(GetPath(button.transform))
                        .Append(" label='").Append(label).Append("'")
                        .Append(" target=").Append(target)
                        .Append(" sprite=").Append(sprite)
                        .Append(" methods=").Append(methods)
                        .Append(" children=");

                    Image[] images = button.GetComponentsInChildren<Image>(true);
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (i > 0) output.Append('|');
                        output.Append(GetPath(images[i].transform, button.transform))
                            .Append(':').Append(images[i].sprite == null ? "null" : images[i].sprite.name)
                            .Append(':').Append(images[i].enabled ? "on" : "off");
                    }
                    output.AppendLine();
                }
            }

            Debug.Log(output.ToString());
        }

        private static void DumpSubtree(Scene scene, string path)
        {
            Transform root = FindPath(scene, path);
            if (root == null)
            {
                Debug.Log($"[ReUI Subtree] MISSING={path}");
                return;
            }

            var output = new StringBuilder();
            output.AppendLine($"[ReUI Subtree] ROOT={path}");
            Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < nodes.Length; i++)
            {
                Transform node = nodes[i];
                Image image = node.GetComponent<Image>();
                Text text = node.GetComponent<Text>();
                Button button = node.GetComponent<Button>();
                RectTransform rect = node as RectTransform;
                output.Append("NODE path=").Append(GetPath(node, root.parent));
                if (image != null)
                    output.Append(" image=").Append(image.sprite == null ? "null" : image.sprite.name)
                        .Append(':').Append(image.enabled ? "on" : "off")
                        .Append(":a=").Append(image.color.a.ToString("0.00"));
                if (text != null)
                    output.Append(" text='").Append(text.text.Replace('\n', ' ')).Append("'");
                if (button != null)
                    output.Append(" button target=").Append(button.targetGraphic == null ? "null" : button.targetGraphic.name);
                if (rect != null)
                    output.Append(" rect=").Append(rect.rect.width.ToString("0")).Append('x').Append(rect.rect.height.ToString("0"));
                output.AppendLine();
            }
            Debug.Log(output.ToString());
        }

        private static Transform FindPath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            GameObject root = null;
            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (candidate.name == parts[0])
                {
                    root = candidate;
                    break;
                }
            }
            if (root == null) return null;
            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null) return null;
            }
            return current;
        }

        private static string GetPath(Transform transform, Transform stop = null)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null && current != stop)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
