using UnityEngine;
using ShipEditor.Model;
using Zenject;
using UnityEngine.UI;
using Constructor;
using Constructor.Satellites;
using Constructor.Ships;
using Services.Resources;
using Services.Localization;
using Services.Gui;
using Gui.Utils;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using System;
using System.IO;
using System.Linq;

namespace ShipEditor.UI
{
    public class BuildsPanel : MonoBehaviour
    {
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IShipEditorModel _shipEditor;
        [Inject] private readonly IGuiManager _guiManager;
        [Inject] private readonly CommandList _commandList;
        [Inject] private readonly IDatabase _database;

        [SerializeField] private LayoutGroup _itemsLayoutGroup;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private InputField _newPresetName;

        private IShipPreset _selectedItem;

        private void OnEnable()
        {
            EnsureExchangeButtons();
            _shipEditor.Events.ShipChanged += OnShipChanged;
            UpdateContent();
            UpdateButtons();
        }

        private void OnDisable()
        {
            _shipEditor.Events.ShipChanged -= OnShipChanged;
        }

        public void OnNewPresetSelected(bool selected)
        {
            if (!selected) return;
            _selectedItem = null;
            UpdateButtons();
        }

        public void OnPresetSelected(IShipPreset preset)
		{
            _selectedItem = preset;
            UpdateButtons();
		}

		public bool Visible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

        public void SavePreset()
        {
            if (_selectedItem == null)
            {
                var preset = _shipEditor.Presets.Create(_shipEditor.Ship.Model.OriginalShip);
                _shipEditor.SaveShipToPreset(preset);
                preset.Name = _newPresetName.text;
                _shipEditor.Presets.Update(preset);
                UpdateContent();
            }
            else
            {
                _guiManager.ShowConfirmationDialog(_localization.GetString("$OverwritePresetConfirmation"),
                    () =>
                    {
                        _shipEditor.SaveShipToPreset(_selectedItem);
                        _shipEditor.Presets.Update(_selectedItem);
                        UpdateContent();
                    });
            }
        }

        public void LoadPreset()
        {
            LoadPreset(_selectedItem);
            _commandList.Clear();
        }

        public void DeletePreset()
        {
            _guiManager.ShowConfirmationDialog(_localization.GetString("$DeletePresetConfirmation"), () =>
            {
                _shipEditor.Presets.Delete(_selectedItem);
                _selectedItem = null;
                UpdateContent();
                UpdateButtons();
            });
        }

        private void LoadPreset(IShipPreset preset)
        {
            if (!_shipEditor.LoadShipFromPreset(preset))
                _guiManager.ShowMessage(_localization.GetString("$PartiallyLoadedPreset"));
        }

        public void ExportLayout()
        {
            var customName = string.IsNullOrWhiteSpace(_newPresetName.text) ? _shipEditor.Ship.Model.OriginalShip.Name : _newPresetName.text;
            if (!LayoutExchange.Export(_shipEditor, customName, out var outputPath, out var error))
            {
                _guiManager.ShowMessage("导出失败：" + error);
                return;
            }

            _guiManager.ShowMessage("已导出到：" + outputPath);
        }

        public void ImportLayout()
        {
            PickImportFile(path =>
            {
                var result = LayoutExchange.Import(_database, _shipEditor, path, out var message);
                switch (result)
                {
                    case LayoutExchange.ImportResult.Success:
                        _commandList.Clear();
                        _guiManager.ShowMessage("导入成功");
                        break;
                    case LayoutExchange.ImportResult.Partial:
                        _commandList.Clear();
                        _guiManager.ShowMessage(message);
                        break;
                    case LayoutExchange.ImportResult.WrongShip:
                    case LayoutExchange.ImportResult.InvalidFile:
                    case LayoutExchange.ImportResult.Failed:
                        _guiManager.ShowMessage(message);
                        break;
                }
            });
        }

        private void OnShipChanged(IShip ship)
        {
            _selectedItem = null;
            UpdateContent();
        }

        private void UpdateButtons()
        {
            _loadButton.gameObject.SetActive(_selectedItem != null);
            _deleteButton.gameObject.SetActive(_selectedItem != null);
        }

        private void UpdateContent()
        {
            var presets = _shipEditor.Presets.GetPresets(_shipEditor.Ship.Model.OriginalShip);
            _itemsLayoutGroup.transform.InitializeElements<ShipPresetItem, IShipPreset>(presets, UpdatePresets);
        }

        private void UpdatePresets(ShipPresetItem item, IShipPreset preset)
        {
            item.Initialize(preset, _resourceLocator, _localization);
        }

        private void EnsureExchangeButtons()
        {
            if (_saveButton == null || _saveButton.transform.parent == null)
                return;

            var parent = _saveButton.transform.parent;
            _exportButton = parent.Find("ExportLayoutButton")?.GetComponent<Button>() ??
                            CreateExtraButton("ExportLayoutButton", "导出配置", ExportLayout, _saveButton);
            _importButton = parent.Find("ImportLayoutButton")?.GetComponent<Button>() ??
                            CreateExtraButton("ImportLayoutButton", "导入配置", ImportLayout, _loadButton != null ? _loadButton : _saveButton);
        }

        private static Button CreateExtraButton(string name, string text, UnityEngine.Events.UnityAction action, Button template)
        {
            var buttonObject = Instantiate(template.gameObject, template.transform.parent, false);
            buttonObject.name = name;
            var button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);

            var label = buttonObject.GetComponentsInChildren<Text>(true).FirstOrDefault();
            if (label != null)
                label.text = text;

            buttonObject.SetActive(true);
            return button;
        }

        private void PickImportFile(Action<string> callback)
        {
            var permission = NativeFilePicker.PickFileWithForcedPermission(path =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _guiManager.ShowMessage("未选择配置文件");
                    return;
                }
                callback?.Invoke(path);
            }, "*/*");

            // On Android the forced helper falls back to the document provider
            // when legacy storage permission is unavailable.  A non-granted
            // result here therefore means the picker itself was busy/failed.
            if (permission != NativeFilePicker.Permission.Granted)
                _guiManager.ShowMessage("无法打开系统文件选择器，请授予存储读取权限后重试");
        }

        private Button _exportButton;
        private Button _importButton;

        private static class LayoutExchange
        {
            public enum ImportResult
            {
                Success,
                Partial,
                Cancelled,
                InvalidFile,
                WrongShip,
                Failed,
            }

            public static bool Export(IShipEditorModel shipEditor, string customName, out string outputPath, out string error)
            {
                outputPath = null;
                error = null;

                try
                {
                    var data = CreateData(shipEditor, customName);
                    var directory = GetExportDirectory();
                    Directory.CreateDirectory(directory);

                    var ship = shipEditor.Ship.Model.OriginalShip;
                    var presetName = SanitizeFileName(string.IsNullOrWhiteSpace(customName)
                        ? "默认布局"
                        : customName);
                    var shipName = SanitizeFileName(ship.Name);
                    var variant = shipEditor.Ship is EditorModeShip editorShip
                        ? "改型" + editorShip.BuildId
                        : "当前配置";
                    outputPath = Path.Combine(directory,
                        $"{shipName}_{ship.Id.Value}_{variant}_{presetName}.shiplayout.json");
                    File.WriteAllText(outputPath, JsonUtility.ToJson(data, true));
                    return true;
                }
                catch (Exception e)
                {
                    error = e.Message;
                    return false;
                }
            }

            public static ImportResult Import(IDatabase database, IShipEditorModel shipEditor, string path, out string message)
            {
                message = null;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return ImportResult.Cancelled;

                try
                {
                    var json = File.ReadAllText(path);
                    var data = JsonUtility.FromJson<ShipLayoutFile>(json);
                    if (data == null || data.shipId <= 0)
                    {
                        message = "配置文件无效";
                        return ImportResult.InvalidFile;
                    }

                    if (shipEditor.Ship.Model.OriginalShip.Id.Value != data.shipId)
                    {
                        var targetShip = shipEditor.Inventory.Ships
                            .FirstOrDefault(item => item.Model.OriginalShip.Id.Value == data.shipId);
                        if (targetShip == null)
                        {
                            message = "该配置对应另一艘舰船，请先切换到对应舰船";
                            return ImportResult.WrongShip;
                        }

                        shipEditor.SelectShip(targetShip);
                    }

                    var preset = CreatePreset(database, data);
                    var fullLoaded = shipEditor.LoadShipFromPreset(preset);
                    if (fullLoaded)
                        return ImportResult.Success;

                    message = "已导入，但缺少部分组件或卫星，空位已保留";
                    return ImportResult.Partial;
                }
                catch (Exception e)
                {
                    message = e.Message;
                    return ImportResult.Failed;
                }
            }

            private static ShipLayoutFile CreateData(IShipEditorModel shipEditor, string customName)
            {
                var preset = new ShipPreset(shipEditor.Ship.Model.OriginalShip) { Name = customName };
                shipEditor.SaveShipToPreset(preset);

                return new ShipLayoutFile
                {
                    shipId = shipEditor.Ship.Model.OriginalShip.Id.Value,
                    shipName = shipEditor.Ship.Model.OriginalShip.Name,
                    presetName = customName,
                    components = preset.Components.Select(Serialize).ToArray(),
                    firstSatellite = Serialize(preset.FirstSatellite),
                    secondSatellite = Serialize(preset.SecondSatellite),
                };
            }

            private static IShipPreset CreatePreset(IDatabase database, ShipLayoutFile data)
            {
                var ship = database.GetShip(new ItemId<Ship>(data.shipId));
                var preset = new ShipPreset(ship)
                {
                    Name = string.IsNullOrWhiteSpace(data.presetName) ? data.shipName : data.presetName,
                    FirstSatellite = DeserializeSatellite(database, data.firstSatellite),
                    SecondSatellite = DeserializeSatellite(database, data.secondSatellite),
                };
                preset.Components.Assign((data.components ?? Array.Empty<ShipLayoutComponent>()).Select(item => Deserialize(database, item)));
                return preset;
            }

            private static CommonSatellite DeserializeSatellite(IDatabase database, ShipLayoutSatellite data)
            {
                if (data == null || data.satelliteId <= 0)
                    return null;

                var satellite = database.GetSatellite(new ItemId<Satellite>(data.satelliteId));
                if (satellite == Satellite.DefaultValue)
                    return null;

                return new CommonSatellite(satellite, (data.components ?? Array.Empty<ShipLayoutComponent>()).Select(item => Deserialize(database, item)));
            }

            private static ShipLayoutSatellite Serialize(ISatellite satellite)
            {
                if (satellite == null)
                    return null;

                return new ShipLayoutSatellite
                {
                    satelliteId = satellite.Information.Id.Value,
                    components = satellite.Components.Select(Serialize).ToArray(),
                };
            }

            private static ShipLayoutComponent Serialize(IntegratedComponent component)
            {
                return new ShipLayoutComponent
                {
                    component = component.Info.SerializeToInt64(),
                    x = component.X,
                    y = component.Y,
                    barrelId = component.BarrelId,
                    keyBinding = component.KeyBinding,
                    behaviour = component.Behaviour,
                    locked = component.Locked,
                    rotation = component.Rotation,
                };
            }

            private static IntegratedComponent Deserialize(IDatabase database, ShipLayoutComponent component)
            {
                var info = ComponentInfo.FromInt64(database, component.component);
                return new IntegratedComponent(info, component.x, component.y, component.barrelId, component.keyBinding,
                    component.behaviour, component.locked, component.rotation);
            }

            private static string GetExportDirectory()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return "/storage/emulated/0/Download";
#else
                var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                return Directory.Exists(downloads) ? downloads : Path.Combine(Application.persistentDataPath, "ShipLayouts");
#endif
            }

            private static string SanitizeFileName(string fileName)
            {
                foreach (var c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');

                return string.IsNullOrWhiteSpace(fileName) ? "ship_layout" : fileName;
            }

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
                public int rotation;
            }
        }
    }
}
