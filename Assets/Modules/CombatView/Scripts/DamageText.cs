using System;
using UnityEngine;

namespace CombatView
{
    // Copied from legacy Combat.Component.Stats.DamageIndicator (migration phase 5b, step 4): one ship's damage text.
    // The core's damage events add to one sum per kind (slot 0 the shield, slot 1 + n damage type n). A sum over 1 shows
    // once no damage of its kind came since the last frame, at most every 0.2 s; the ship's removal shows the rest.
    internal sealed class DamageText
    {
        // colors: by slot. show: the amount and its colour, at the ship.
        public DamageText(Color[] colors, Action<float, Color> show)
        {
            _colors = colors;
            _show = show;
            _damage = new float[colors.Length];
            _changed = new bool[colors.Length];
        }

        // damageType -1 is the shield. Unknown types are left out.
        public void ApplyDamage(int damageType, float amount)
        {
            var slot = damageType + 1;
            if (slot < 0 || slot >= _damage.Length || !(amount > 0f))
                return;

            _changed[slot] = true;
            _damage[slot] += amount;
        }

        public void Dispose()
        {
            for (var i = 0; i < _damage.Length; ++i)
                if (_damage[i] > 1f)
                    _show(_damage[i], _colors[i]);
        }

        public void Update(float elapsedTime)
        {
            _currentTime += elapsedTime;
            if (_currentTime - _lastShowTime > _cooldown)
            {
                _lastShowTime = _currentTime;

                for (var i = 0; i < _damage.Length; ++i)
                {
                    if (_damage[i] > 1f && !_changed[i])
                    {
                        _show(_damage[i], _colors[i]);
                        _damage[i] = 0;
                    }
                }
            }

            Array.Clear(_changed, 0, _changed.Length);
        }

        // Legacy EffectFactory.CreateDamageTextEffect.
        public static string Format(float damage)
        {
            if (damage < 1e3f)
                return ((int)damage).ToString();
            if (damage < 1e6f)
                return (int)(damage / 1e3f) + "K";
            return (int)(damage / 1e6f) + "M";
        }

        private float _lastShowTime;
        private float _currentTime;

        private const float _cooldown = 0.2f;

        private readonly float[] _damage;
        private readonly bool[] _changed;
        private readonly Color[] _colors;
        private readonly Action<float, Color> _show;
    }
}
