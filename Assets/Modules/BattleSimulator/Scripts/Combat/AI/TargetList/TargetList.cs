using Combat.Component.Ship;
using Combat.Scene;

namespace Combat.Ai
{
    public class TargetList : TargetListBase
    {
        public TargetList(IScene scene)
            : base(scene)
        {
        }

        public void Update(float elapsedTime, IShip ship, IShip enemy)
        {
            _cooldown -= elapsedTime;

            if (_cooldown > 0)
                return;

            _cooldown = UpdateInterval;

            base.Update(ship, enemy, true, true);
        }
        private float _cooldown;
        private const float UpdateInterval = 1.0f;
    }
}
