using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using CommonComponents.Utils;
using Helpers = GameModel.Serialization.Helpers;

namespace Constructor.Satellites
{
    public class CommonSatellite : ISatellite
    {
        public static CommonSatellite Create(SatelliteBuild build)
        {
            if (build == null) return null;
            return new CommonSatellite(build);
        }

        public CommonSatellite(Satellite satellite, IEnumerable<IntegratedComponent> components)
        {
            BaseModel = satellite;
            _components.Assign(components);
            _components.DataChangedEvent += OnDataChanged;
        }

        public CommonSatellite(SatelliteBuild build)
        {
            BaseModel = build.Satellite;
            _components.Assign(build.Components.Select<InstalledComponent,IntegratedComponent>(ComponentExtensions.FromDatabase));
            _components.DataChangedEvent += OnDataChanged;
        }

		public string Name => BaseModel.Name;
        public Satellite BaseModel { get; }
        [System.Obsolete("Use BaseModel")] public Satellite Information => BaseModel;

        public IItemCollection<IntegratedComponent> Components { get { return _components; } }

        public bool DataChanged { get; set; }

        private void OnDataChanged()
        {
            DataChanged = true;
        }

        private ObservableCollection<IntegratedComponent> _components = new ObservableCollection<IntegratedComponent>();
    }
}
