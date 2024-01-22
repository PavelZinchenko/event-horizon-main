using Combat.Component.Features;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Combat.Component.Systems.Devices
{
    public class CamouflageDevice : SystemBase, IDevice, IFeaturesModification
    {
        public CamouflageDevice(float chanceToAvoidDrone, float chanceToAvoidMissile, DeviceClass deviceClass)
            : base(-1, SpriteId.Empty)
        {
			DeviceClass = deviceClass;
            _chanceToAvoidDrone = UnityEngine.Mathf.Clamp01(chanceToAvoidDrone);
            _chanceToAvoidMissile = UnityEngine.Mathf.Clamp01(chanceToAvoidMissile);
        }

        public override bool CanBeActivated => false;
		public DeviceClass DeviceClass { get; }

		public override IFeaturesModification FeaturesModification { get { return this; } }
        public bool TryApplyModification(ref FeaturesData data)
        {
            if (data.ChanceToAvoidDrone < _chanceToAvoidDrone)
                data.ChanceToAvoidDrone = _chanceToAvoidDrone;
            if (data.ChanceToAvoidMissile < _chanceToAvoidMissile)
                data.ChanceToAvoidMissile = _chanceToAvoidMissile;
    
            return true;
        }
        public void Deactivate() {}
        protected override void OnDispose() { }
        protected override void OnUpdateView(float elapsedTime) {}
        protected override void OnUpdatePhysics(float elapsedTime) {}

        private readonly float _chanceToAvoidDrone;
        private readonly float _chanceToAvoidMissile;
    }
}
