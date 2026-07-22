using System;
using System.IO;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using CommonComponents.Utils;
using UnityEngine;

namespace Constructor.Ships
{
    public class EditorModeShip : BaseShip
    {
        public EditorModeShip(ShipBuild build, IDatabase database)
            : base(new ShipModel(LoadOverride(build, database), database), build.CustomAI)
        {
            _database = database;
            _shipBuild = build;
            if (build.LeftSatelliteBuild != null) FirstSatellite = new Satellites.EditorModeSatellite(build.LeftSatelliteBuild, database);
            if (build.RightSatelliteBuild != null) SecondSatellite = new Satellites.EditorModeSatellite(build.RightSatelliteBuild, database);
        }

        public override IItemCollection<IntegratedComponent> Components
        {
            get
            {
                if (_components == null)
                {
                    _components = new ObservableCollection<IntegratedComponent>(_shipBuild.Components.Select<InstalledComponent, IntegratedComponent>(item =>
                    {
                        var component = ComponentExtensions.FromDatabase(item);
                        component.Locked = false;
                        return component;
                    }));
                    _components.DataChangedEvent += SaveComponents;
                }
                return _components;
            }
        }

        public int BuildId => _shipBuild.Id.Value;
        public override string Name { get => _shipBuild.Id.ToString(); set { } }

        private void SaveComponents()
        {
            _shipBuild.SetComponents(_components.Select(ToDatabaseModel));
            _database.SaveShipBuild(_shipBuild.Id);
            SaveOverride();
        }

        private static ShipBuild LoadOverride(ShipBuild build, IDatabase database)
        {
            try
            {
                var path = GetOverridePath(build.Id.Value);
                if (!File.Exists(path)) return build;
                var data = JsonUtility.FromJson<ComponentOverride>(File.ReadAllText(path));
                if (data?.Components == null) return build;
                build.SetComponents(data.Components.Select(item =>
                    ToDatabaseModel(ComponentExtensions.Deserialize(database, Convert.FromBase64String(item)))));
            }
            catch (Exception exception) { Debug.LogWarning("Unable to load ship build override: " + exception.Message); }
            return build;
        }

        private void SaveOverride()
        {
            try
            {
                var path = GetOverridePath(BuildId);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var data = new ComponentOverride
                {
                    Components = _components.Select(item => Convert.ToBase64String(item.Serialize().ToArray())).ToArray()
                };
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception) { Debug.LogWarning("Unable to save ship build override: " + exception.Message); }
        }

        private static string GetOverridePath(int buildId) =>
            Path.Combine(Application.persistentDataPath, "DefaultShipBuildOverrides", buildId + ".json");

        private static InstalledComponent ToDatabaseModel(IntegratedComponent component) =>
            new InstalledComponent(component.Info.Data, component.Info.ModificationType,
                component.Info.ModificationQuality, component.X, component.Y, component.BarrelId, component.Behaviour,
                component.KeyBinding, component.Rotation);

        [Serializable]
        private class ComponentOverride { public string[] Components; }

        private ObservableCollection<IntegratedComponent> _components;
        private readonly ShipBuild _shipBuild;
        private readonly IDatabase _database;
    }
}
