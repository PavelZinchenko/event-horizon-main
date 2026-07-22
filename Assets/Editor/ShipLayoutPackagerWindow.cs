using System;
using System.IO;
using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Imports the .shiplayout.json files exported by the in-game ship editor and
/// writes their component layout into a database ShipBuild resource.
/// </summary>
public sealed class ShipLayoutPackagerWindow : EditorWindow
{
    private const string BuildFolder = "Assets/Modules/Database/Resources/Database/Ship/Builds";
    private string _layoutPath = string.Empty;
    private string _targetBuildPath = string.Empty;
    private ShipLayoutFile _layout;
    private Vector2 _scroll;
    private string _status = "请选择手机端导出的 .shiplayout.json 文件。";

    [MenuItem("Tools/三体视界/舰船预设打包器")]
    public static void Open()
    {
        var window = GetWindow<ShipLayoutPackagerWindow>("舰船预设打包器");
        window.minSize = new Vector2(570, 330);
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.LabelField("手机预设 → 游戏默认舰船", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("先在手机游戏内导出舰船配置，再在这里选择导出文件和要覆盖的默认 ShipBuild。写入时会自动备份原文件。", MessageType.Info);

        EditorGUILayout.Space();
        DrawPath("导出配置", _layoutPath, PickLayout);
        using (new EditorGUI.DisabledScope(_layout == null))
            DrawPath("目标默认舰船", _targetBuildPath, PickTargetBuild);

        if (_layout != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("舰船 ID", _layout.shipId.ToString());
            EditorGUILayout.LabelField("舰船名称", EmptyAsDash(_layout.shipName));
            EditorGUILayout.LabelField("预设名称", EmptyAsDash(_layout.presetName));
            EditorGUILayout.LabelField("组件数量", (_layout.components?.Length ?? 0).ToString());
            if (_layout.firstSatellite != null || _layout.secondSatellite != null)
                EditorGUILayout.HelpBox("主舰组件会完整写入。卫星在数据库中是独立 ShipBuild 引用，本工具不会覆盖已有卫星配置。", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(_status, MessageType.None);
        using (new EditorGUI.DisabledScope(_layout == null || string.IsNullOrEmpty(_targetBuildPath)))
        {
            if (GUILayout.Button("写入默认预设", GUILayout.Height(34)))
                ApplyLayout();
        }

        using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer))
        {
            if (GUILayout.Button("构建 Android APK", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("构建 APK", "确认使用当前资源构建 Android APK？", "构建", "取消"))
                    AndroidDevelopmentBuild.Build();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawPath(string label, string path, Action pick)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(path) ? "未选择" : path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if (GUILayout.Button("选择…", GUILayout.Width(72))) pick();
        EditorGUILayout.EndHorizontal();
    }

    private void PickLayout()
    {
        var path = EditorUtility.OpenFilePanel("选择手机端导出的舰船配置", GetInitialDirectory(_layoutPath), "json");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var layout = JsonUtility.FromJson<ShipLayoutFile>(File.ReadAllText(path));
            if (layout == null || layout.shipId <= 0 || layout.components == null)
                throw new InvalidDataException("文件缺少 shipId 或 components，不是有效的舰船配置。");

            _layoutPath = path;
            _layout = layout;
            _targetBuildPath = FindMatchingBuild(layout.shipId) ?? string.Empty;
            _status = string.IsNullOrEmpty(_targetBuildPath)
                ? "配置读取成功，但没有自动找到同舰船 ID 的默认 ShipBuild，请手动选择。"
                : "配置读取成功，已自动匹配一个同舰船 ID 的默认 ShipBuild。";
        }
        catch (Exception exception)
        {
            _layout = null;
            _targetBuildPath = string.Empty;
            _status = "读取失败：" + exception.Message;
        }
    }

    private void PickTargetBuild()
    {
        var absoluteFolder = Path.GetFullPath(BuildFolder);
        var path = EditorUtility.OpenFilePanel("选择要覆盖的默认 ShipBuild", absoluteFolder, "json");
        if (string.IsNullOrEmpty(path)) return;
        _targetBuildPath = ToProjectPath(path);
        if (Path.IsPathRooted(_targetBuildPath))
        {
            _status = "目标必须位于当前 Unity 项目的 Ship/Builds 目录中。";
            _targetBuildPath = string.Empty;
            return;
        }
        ValidateTarget(false);
    }

    private void ApplyLayout()
    {
        try
        {
            var target = ValidateTarget(true);
            if (target == null) return;

            var components = _layout.components.Select(ConvertComponent).ToArray();
            if (components.Any(item => item.ComponentId <= 0))
                throw new InvalidDataException("配置中包含无效组件 ID。");

            var absoluteTarget = Path.GetFullPath(_targetBuildPath);
            var backupDirectory = Path.GetFullPath(Path.Combine("Backups", "ShipLayouts"));
            Directory.CreateDirectory(backupDirectory);
            var backupName = Path.GetFileNameWithoutExtension(absoluteTarget) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            File.Copy(absoluteTarget, Path.Combine(backupDirectory, backupName), true);

            target.Components = components;
            File.WriteAllText(absoluteTarget, JsonUtility.ToJson(target, true) + Environment.NewLine);
            AssetDatabase.ImportAsset(_targetBuildPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            _status = $"写入成功：{components.Length} 个组件已成为 Build {target.Id} 的默认布局。备份：Backups/ShipLayouts/{backupName}";
            Debug.Log(_status);
        }
        catch (Exception exception)
        {
            _status = "写入失败：" + exception.Message;
            Debug.LogException(exception);
        }
    }

    private ShipBuildSerializable ValidateTarget(bool showDialog)
    {
        try
        {
            var absolute = Path.GetFullPath(_targetBuildPath);
            var allowedFolder = Path.GetFullPath(BuildFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!absolute.StartsWith(allowedFolder, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("目标必须位于 Assets/Modules/Database/Resources/Database/Ship/Builds 中。");
            if (!File.Exists(absolute)) throw new FileNotFoundException("目标 ShipBuild 不存在。", absolute);
            var build = JsonUtility.FromJson<ShipBuildSerializable>(File.ReadAllText(absolute));
            if (build == null || build.Id <= 0 || build.ShipId <= 0) throw new InvalidDataException("目标文件不是有效的 ShipBuild。 ");
            if (_layout != null && build.ShipId != _layout.shipId)
            {
                var message = $"舰船 ID 不一致：导出配置为 {_layout.shipId}，目标 ShipBuild 为 {build.ShipId}。";
                if (showDialog) EditorUtility.DisplayDialog("不能写入", message, "确定");
                _status = message;
                return null;
            }
            _status = $"目标有效：Build {build.Id}，Ship {build.ShipId}。";
            return build;
        }
        catch (Exception exception)
        {
            _status = "目标无效：" + exception.Message;
            if (showDialog) EditorUtility.DisplayDialog("不能写入", _status, "确定");
            return null;
        }
    }

    private static InstalledComponentSerializable ConvertComponent(ShipLayoutComponent source)
    {
        var packed = unchecked((ulong)source.component);
        return new InstalledComponentSerializable
        {
            ComponentId = (int)(packed >> 32),
            Quality = (ModificationQuality)((packed >> 24) & 0xff),
            Modification = (int)((packed >> 16) & 0xff),
            X = source.x,
            Y = source.y,
            BarrelId = source.barrelId,
            KeyBinding = source.keyBinding,
            Behaviour = source.behaviour,
        };
    }

    private static string FindMatchingBuild(int shipId)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:TextAsset", new[] { BuildFolder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                var build = JsonUtility.FromJson<ShipBuildSerializable>(File.ReadAllText(path));
                if (build != null && build.ShipId == shipId) return path;
            }
            catch
            {
                // Ignore unrelated or temporarily incomplete JSON resources.
            }
        }
        return null;
    }

    private static string ToProjectPath(string absolutePath)
    {
        var normalized = absolutePath.Replace('\\', '/');
        var project = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/').TrimEnd('/');
        return normalized.StartsWith(project + "/", StringComparison.OrdinalIgnoreCase)
            ? normalized.Substring(project.Length + 1)
            : absolutePath;
    }

    private static string GetInitialDirectory(string path) => string.IsNullOrEmpty(path)
        ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        : Path.GetDirectoryName(path);

    private static string EmptyAsDash(string value) => string.IsNullOrWhiteSpace(value) ? "—" : value;

    [Serializable]
    private sealed class ShipLayoutFile
    {
        public int shipId;
        public string shipName;
        public string presetName;
        public ShipLayoutComponent[] components;
        public ShipLayoutSatellite firstSatellite;
        public ShipLayoutSatellite secondSatellite;
    }

    [Serializable]
    private sealed class ShipLayoutSatellite
    {
        public int satelliteId;
        public ShipLayoutComponent[] components;
    }

    [Serializable]
    private sealed class ShipLayoutComponent
    {
        public long component;
        public int x;
        public int y;
        public int barrelId;
        public int keyBinding;
        public int behaviour;
        public bool locked;
    }
}
