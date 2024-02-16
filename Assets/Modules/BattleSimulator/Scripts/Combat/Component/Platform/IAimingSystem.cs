using Combat.Component.Unit;

namespace Combat.Component.Platform
{
    public interface IAimingSystem
    {
		IUnit ActiveTarget { get; set; }
		void Aim(float bulletVelocity, float weaponRange, float relativeEffect);
    }
}
