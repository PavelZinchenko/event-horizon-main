﻿using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public class EditorModeShip : BaseShip
    {
        public EditorModeShip(ShipBuild build, IDatabase database)
            : base(new ShipModel(build, database), build.CustomAI)
        {
            _database = database;
            _shipBuild = build;

            if (build.LeftSatelliteBuild != null)
                FirstSatellite = new Satellites.EditorModeSatellite(build.LeftSatelliteBuild, database);
            if (build.RightSatelliteBuild != null)
                SecondSatellite = new Satellites.EditorModeSatellite(build.RightSatelliteBuild, database);
        }

        public override IItemCollection<IntegratedComponent> Components
        {
            get
            {
                if (_components == null)
                {
                    _components = new ObservableCollection<IntegratedComponent>(_shipBuild.Components.Select<InstalledComponent,IntegratedComponent>(item =>
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

        public override string Name { get { return _shipBuild.Id.ToString(); } set {} }

        private void SaveComponents()
        {
            UnityEngine.Debug.Log("EditorModeShip.SaveComponents");
            _shipBuild.SetComponents(_components.Select(item => ToDatabaseModel(item)));
            _database.SaveShipBuild(_shipBuild.Id);
        }

        private static InstalledComponent ToDatabaseModel(IntegratedComponent component)
        {
            return new InstalledComponent(component.Info.Data, component.Info.ModificationType,
                component.Info.ModificationQuality, component.X, component.Y, component.BarrelId, component.Behaviour, component.KeyBinding);
        }

        private ObservableCollection<IntegratedComponent> _components;
        private readonly ShipBuild _shipBuild;
        private readonly IDatabase _database;
    }
}
