using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using CommonComponents.Utils;

namespace Constructor.Satellites
{
    public class EditorModeSatellite : ISatellite
    {
        public EditorModeSatellite(SatelliteBuild build, IDatabase database)
        {
            _database = database;
            _build = build;
        }

		public string Name => _build.Satellite.Name + " #" + _build.Id.Value;
        public Satellite Information => _build.Satellite;

		public IItemCollection<IntegratedComponent> Components
		{
			get
			{
				if (_components == null)
				{
					_components = new ObservableCollection<IntegratedComponent>(_build.Components.Select(item =>
					{
						var component = ComponentExtensions.FromDatabase(item);
						component.Locked = false;
						return component;
					}));

					_components.DataChangedEvent += OnDataChanged;
				}

				return _components;
			}
		}

        public bool DataChanged { get => false; set {} }

        private void OnDataChanged()
        {
            UnityEngine.Debug.Log("EditorModeSatellite.OnDataChanged");
            _build.SetComponents(_components.Select(item => ToDatabaseModel(item)));
            _database.SaveSatelliteBuild(_build.Id);
        }

        private ObservableCollection<IntegratedComponent> _components;
        private readonly SatelliteBuild _build;
        private readonly IDatabase _database;

        private InstalledComponent ToDatabaseModel(IntegratedComponent component)
        {
            return new InstalledComponent(component.Info.Data, component.Info.ModificationType, 
                component.Info.ModificationQuality, component.X, component.Y, component.BarrelId, component.Behaviour, component.KeyBinding);
        }
	}
}
