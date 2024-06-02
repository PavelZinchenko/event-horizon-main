using System.Collections.Generic;
using System.Linq;
using Constructor.Satellites;
using GameDatabase.DataModel;
using CommonComponents.Utils;

namespace Constructor.Ships
{
    public class CommonShip : BaseShip
    {
        public CommonShip(IShipModel data, IEnumerable<IntegratedComponent> components)
            : base(data)
        {
            _components.Assign(components);
            _components.DataChangedEvent += OnDataChanged;
        }

        public CommonShip(Ship data, IEnumerable<IntegratedComponent> components)
            : base(new ShipModel(data))
        {
            _components.Assign(components);
            _components.DataChangedEvent += OnDataChanged;
        }

        public CommonShip(ShipBuild build, GameDatabase.IDatabase database)
            : base(new ShipModel(build, database), build.CustomAI)
        {
            const float maxSaturation = 0.25f;

            if (build.LeftSatelliteBuild != null)
                FirstSatellite = new CommonSatellite(build.LeftSatelliteBuild);
            if (build.RightSatelliteBuild != null)
                SecondSatellite = new CommonSatellite(build.RightSatelliteBuild);

            _components.Assign(build.Components.Select<InstalledComponent,IntegratedComponent>(ComponentExtensions.FromDatabase));
            _components.DataChangedEvent += OnDataChanged;

            if (build.RandomColor)
            {
                ColorScheme.Type = ShipColorScheme.SchemeType.Hsv;
                ColorScheme.Hue = UnityEngine.Random.value;
                ColorScheme.Saturation = UnityEngine.Random.value * maxSaturation;
            }
        }

        public override string Name
        {
            get { return string.IsNullOrEmpty(_name) ? base.Name : _name; }
            set
            {
                _name = value;
                DataChanged = true;
            }
        }

        public override IItemCollection<IntegratedComponent> Components { get { return _components; } }

        private void OnDataChanged()
        {
            DataChanged = true;
        }

        private readonly ObservableCollection<IntegratedComponent> _components = new ObservableCollection<IntegratedComponent>();
        private string _name;
    }
}
