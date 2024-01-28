using Combat.Component.Bullet.Cooldown;

namespace Combat.Component.Bullet.Action
{
    public static class IActionExtensions
    {
        public static IAction WithCooldown(this IAction action, ICooldown cooldown) =>
            cooldown == null ? action : new ActionWithCooldown(action, cooldown);
        
        public static IAction WithCooldown(this IAction action, float seconds) =>
            seconds <= 0 ? action : new ActionWithCooldown(action, new Cooldown.Cooldown(seconds));
    }
}