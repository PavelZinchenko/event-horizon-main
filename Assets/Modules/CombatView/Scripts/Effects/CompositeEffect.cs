using System.Collections.Generic;
using UnityEngine;

namespace CombatView.Effects
{
    // The visual effect record player (migration phase 5b, step 3), a rewrite of legacy Combat.Effects.CompositeEffect
    // on plain element data. Each element makes Quantity effects under one root. Life runs from 1 to 0 over the record
    // lifetime; each element is visible from its start time for its lifetime, or from its start time on if it loops.
    // Growth makes an element grow by GrowthRate x its size over its life; TurnRate turns it the same way. UseRealTime
    // elements count the time since the start instead of the life, so they keep going when Run sets a shorter lifetime.
    // The pool ticks the record; the record ticks its elements, so they follow it in the same frame.
    public sealed class CompositeEffect : IEffect
    {
        internal CompositeEffect(EffectRecord record, Transform root, EffectPool pool)
        {
            _record = record;
            _root = root;
            _pool = pool;

            foreach (var element in record.Elements)
            {
                for (var i = 0; i < element.Quantity; ++i)
                {
                    var effect = pool.CreateElement(element, root);
                    effect.Visible = false;
                    effect.Color = element.Color;
                    effect.Size = element.Size;
                    effect.Position = element.Offset;
                    effect.Rotation = element.Rotation;
                    _effects.Add(effect);
                }
            }

            Position = Vector2.zero;
            Rotation = 0;
            Life = 1.0f;
            Size = 1.0f;
            IsAlive = true;
        }

        public bool Visible
        {
            get => _root && _root.gameObject.activeSelf;
            set { if (_root) _root.gameObject.SetActive(value); }
        }

        public Vector2 Position { get { return _position; } set { _position = value; _positionChanged = true; } }
        public float Rotation { get { return _rotation; } set { _rotation = value; _positionChanged = true; } }
        public float Size { get { return _size; } set { _size = value; _sizeChanged = true; } }
        public Color Color { get { return _color; } set { _color = value; _colorChanged = true; } }
        public float Life { get { return _life; } set { _life = value; _lifeChanged = true; } }

        public bool IsAlive { get; private set; }

        // A lifetime of 0 plays the whole record.
        public void Run(float lifetime, Vector2 velocity, float angularVelocity)
        {
            Life = 1.0f;
            _lifetime = lifetime > 0.01f ? lifetime : _record.Lifetime;
            _velocity = velocity;
            _angularVelocity = angularVelocity;
            _isAutomatic = true;
        }

        public void OnParentSizeChanged() { }
        public void Restart() { _totalElapsedTime = 0; }

        public void Tick(float deltaTime)
        {
            if (!IsAlive)
                return;

            if (!_root)
            {
                Dispose();
                return;
            }

            _totalElapsedTime += deltaTime;

            if (_isAutomatic)
            {
                Position += _velocity * deltaTime;
                Rotation += _angularVelocity * deltaTime;
                Life -= deltaTime / _lifetime;

                if (Life <= 0)
                {
                    Dispose();
                    return;
                }
            }

            if (_positionChanged)
            {
                EffectMath.Move(_root, Position);
                _root.localEulerAngles = new Vector3(0, 0, Rotation);
                _positionChanged = false;
            }

            if (_sizeChanged)
            {
                _root.localScale = Size * Vector3.one;
                foreach (var effect in _effects)
                    effect.OnParentSizeChanged();
                _sizeChanged = false;
            }

            if (_colorChanged)
            {
                UpdateColor();
                _colorChanged = false;
            }

            if (_lifeChanged)
            {
                UpdateLife();
                _lifeChanged = false;
            }

            // Legacy elements ran their own Update while active.
            foreach (var effect in _effects)
                if (effect.Visible)
                    effect.Tick(deltaTime);
        }

        public void Dispose()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            // A destroyed root (its parent view went first) took its elements with it.
            if (_root)
                foreach (var effect in _effects)
                    effect.Dispose();
            _effects.Clear();
            _pool.ReleaseRoot(_root);
        }

        private void UpdateLife()
        {
            var time = (1f - Life) * _record.Lifetime;
            var index = 0;
            foreach (var element in _record.Elements)
            {
                var elementTime = element.UseRealTime ? _totalElapsedTime : time;
                var visible = IsElementVisible(element, elementTime);
                for (var i = 0; i < element.Quantity; ++i)
                {
                    var effect = _effects[index++];
                    if (!visible)
                    {
                        effect.Visible = false;
                        continue;
                    }

                    effect.Visible = true;

                    if (!element.Loop)
                        UpdateEffectLife(effect, element, elementTime);
                    else
                        UpdateLoopingEffectLife(effect, element, elementTime);

                    if (element.GrowthRate != 0)
                        effect.Size = element.Size * (1.0f + element.GrowthRate * (1.0f - effect.Life));
                    if (element.TurnRate != 0)
                        effect.Rotation = element.Rotation + element.TurnRate * (1.0f - effect.Life);
                }
            }
        }

        private void UpdateColor()
        {
            var index = 0;
            foreach (var element in _record.Elements)
            {
                var color = element.ColorMode.Apply(element.Color, Color);
                for (var i = 0; i < element.Quantity; ++i)
                    _effects[index++].Color = color;
            }
        }

        private static bool IsElementVisible(EffectElement element, float time)
        {
            if (element.Loop)
                return time >= element.StartTime;

            return time >= element.StartTime && time <= element.StartTime + element.Lifetime;
        }

        private static void UpdateLoopingEffectLife(IEffect effect, EffectElement element, float time)
        {
            var life = (time - element.StartTime) / element.Lifetime;
            life -= Mathf.Floor(life);
            if (element.Inverse) life = 1.0f - life;
            var delta = life > effect.Life ? life - effect.Life : effect.Life - life;
            if (delta > 0.5f) effect.Restart();
            effect.Life = life;
        }

        private static void UpdateEffectLife(IEffect effect, EffectElement element, float time)
        {
            var elapsedTime = Mathf.Clamp01((time - element.StartTime) / element.Lifetime);
            effect.Life = element.Inverse ? elapsedTime : 1f - elapsedTime;
        }

        private readonly EffectRecord _record;
        private readonly Transform _root;
        private readonly EffectPool _pool;
        private readonly List<IEffect> _effects = new List<IEffect>();

        private Vector2 _position;
        private float _rotation;
        private bool _positionChanged;

        private float _size;
        private bool _sizeChanged;

        private Color _color;
        private bool _colorChanged;

        private float _life;
        private bool _lifeChanged;

        private Vector2 _velocity;
        private float _angularVelocity;
        private float _lifetime;
        private bool _isAutomatic;

        private float _totalElapsedTime;
    }
}
