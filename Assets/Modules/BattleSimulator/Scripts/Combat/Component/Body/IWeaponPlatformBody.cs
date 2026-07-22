using Combat.Component.Ship;
using Combat.Component.Unit;

namespace Combat.Component.Body
{
    public interface IWeaponPlatformBody : IBody
    {
        float FixedRotation { get; }
        float AutoAimingAngle { get; }
        void Aim(float bulletVelocity, float weaponRange, float relativeEffect);
        IShip ActiveTarget { get; set; }
        IUnit ActiveUnitTarget { get; set; }
    }
}
