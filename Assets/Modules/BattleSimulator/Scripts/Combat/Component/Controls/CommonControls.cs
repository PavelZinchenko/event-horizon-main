namespace Combat.Component.Controls
{
    public class CommonControls : IControls
    {
        public CommonControls()
        {
            _systems = new SystemsState(new(0), OnDataChanged);
        }

        public bool DataChanged { get; set; }
        public SystemsState Systems => _systems;

        public float Throttle
        {
            get { return _throttle; }
            set
            {
				_throttle = value < 0 ? 0 : value > 1 ? 1 : value;
                DataChanged = true;
            }
        }

        public float? Course
        {
            get
            {
                return _course;
            }
            set
            {
                _course = value;
                DataChanged = true;
            }
        }

        public void OnDataChanged()
        {
            DataChanged = true;
        }

        private SystemsState _systems;
        private float _throttle;
        private float? _course;
    }
}
