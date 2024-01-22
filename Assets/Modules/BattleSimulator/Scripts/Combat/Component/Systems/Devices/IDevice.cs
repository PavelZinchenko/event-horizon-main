using GameDatabase.Enums;

namespace Combat.Component.Systems.Devices
{
    public interface IDevice : ISystem
    {
        void Deactivate();
		DeviceClass DeviceClass { get; }
    }
}
