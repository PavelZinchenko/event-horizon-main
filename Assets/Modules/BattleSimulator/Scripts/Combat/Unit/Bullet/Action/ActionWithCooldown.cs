using Combat.Collision;
using Combat.Component.Bullet.Cooldown;

namespace Combat.Component.Bullet.Action
{
    public class ActionWithCooldown : IAction
    {
        public ActionWithCooldown(IAction action, ICooldown cooldown)
        {
            _cooldown = cooldown;
            _action = action;
        }

        public void Dispose()
        {
            _action.Dispose();
        }

        public ConditionType Condition => _action.Condition;
        public CollisionEffect Invoke()
        {
            return _cooldown.TryUpdate() ? _action.Invoke() : CollisionEffect.None;
        }

        private readonly ICooldown _cooldown;
        private readonly IAction _action;
    }
}