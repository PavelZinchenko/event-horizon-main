using Combat.Component.Systems;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Combat.Component.Systems.Devices
{
    public sealed class PassiveRadarDevice : SystemBase, IDevice
    {
        public PassiveRadarDevice(DeviceStats stats) : base(-1, SpriteId.Empty)
        {
            DeviceClass = stats.DeviceClass;
        }

        public override bool CanBeActivated => false;
        public DeviceClass DeviceClass { get; }
        public void Deactivate() { }
        protected override void OnUpdatePhysics(float elapsedTime) { }
        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }
    }

    public sealed class LowDimensionalProjectionDevice : SystemBase, IDevice
    {
        public LowDimensionalProjectionDevice(DeviceStats stats) : base(-1, SpriteId.Empty)
        {
            DeviceClass = stats.DeviceClass;
        }

        public override bool CanBeActivated => false;
        public DeviceClass DeviceClass { get; }
        public void Deactivate() { }
        protected override void OnUpdatePhysics(float elapsedTime) { }
        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }
    }
}
