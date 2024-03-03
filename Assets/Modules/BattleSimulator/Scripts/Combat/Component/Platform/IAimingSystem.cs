using Combat.Component.Ship;

namespace Combat.Component.Platform
{
    public interface IAimingSystem
    {
		IShip ActiveTarget { get; set; }
        void Aim(float bulletVelocity, float weaponRange, float relativeEffect);
    }
}
