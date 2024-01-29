using UnityEngine;

namespace Combat.Component.Bullet.Cooldown
{
    public class Cooldown: ICooldown
    {
        public Cooldown(float cooldown)
        {
            _cooldown = cooldown;
        }

        public bool TryUpdate()
        {
            var time = Time.fixedTime;
            if (_lastUpdateTime > 0 && time - _lastUpdateTime < _cooldown)
                return false;

            _lastUpdateTime = time;
            return true;
        }

        private float _lastUpdateTime;
        private readonly float _cooldown;
    }

}