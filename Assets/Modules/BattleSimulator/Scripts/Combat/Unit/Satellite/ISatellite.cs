using Combat.Component.Platform;
using Combat.Component.Unit;

namespace Combat.Component.Satellite
{
    public interface ISatellite : IUnit
    {
        public IAimingSystem AimingSystem { get; }
        void Destroy();
    }
}
