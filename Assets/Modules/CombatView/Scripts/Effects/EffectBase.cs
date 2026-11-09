using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.EffectBase (migration phase 5b, step 2). Changes: the pool replaces the
    // GameObjectHolder, and Tick with the frame's time replaces Unity's Update.
    public abstract class EffectBase : MonoBehaviour, IEffectComponent
    {
        [SerializeField] private float _alphaScale = 1.0f;
        [SerializeField] private Color _defaultColor = Color.white;

        private Vector2 _position;
        private float _rotation;
        private bool _positionChanged;

        private float _size;
        private float _scale;
        private bool _sizeChanged;

        private float _opacity;
        private Color _color;
        private bool _colorChanged;

        private float _life;
        private bool _lifeChanged;

        private PrefabPool _pool;
        private Renderer _renderer;

        protected Renderer Renderer => (_renderer != null) ? _renderer : (_renderer = GetComponent<Renderer>());

        // Legacy ran one Update when the effect became visible; Tick(0) applies the changes without advancing time.
        public bool Visible
        {
            get { return gameObject.activeSelf; }
            set
            {
                if (gameObject.activeSelf)
                {
                    if (!value) gameObject.SetActive(false);
                }
                else
                {
                    if (value)
                    {
                        gameObject.SetActive(true);
                        Tick(0f);
                    }
                }
            }
        }

        public Vector2 Position { get { return _position; } set { _position = value; _positionChanged = true; } }
        public float Rotation { get { return _rotation; } set { _rotation = value; _positionChanged = true; } }
        public float Size { get { return _size; } set { _size = value; _sizeChanged = true; } }
        public Color Color { get { return _color; } set { _color = value; _colorChanged = true; } }
        public float Life { get { return _life; } set { _life = value; _lifeChanged = true; } }

        public void Initialize(PrefabPool pool)
        {
            _pool = pool;
            IsAutomatic = false;
            Position = Vector2.zero;
            Rotation = 0;
            Velocity = Vector2.zero;
            AngularVelocity = 0;
            Lifetime = 1.0f;
            Life = 1.0f;
            Size = 1.0f;
            Color = _defaultColor;
            Scale = 1.0f;
            Opacity = 1.0f;
            IsAlive = true;
            gameObject.SetActive(true);

            OnInitialize();
        }

        public bool IsAlive { get; private set; }

        public void Run(float lifetime, Vector2 velocity, float angularVelocity)
        {
            IsAutomatic = true;
            Lifetime = lifetime;
            Velocity = velocity;
            AngularVelocity = angularVelocity;

            Life = 1.0f;
        }

        public void Dispose()
        {
            if (!IsAlive)
                return;

            OnDispose();

            IsAlive = false;
            _pool.Release(gameObject);
        }

        public virtual void OnParentSizeChanged() {}
        public virtual void Restart() {}

        protected float Opacity { get { return _opacity; } set { _opacity = value; _colorChanged = true; } }
        protected float Scale { get { return _scale; } set { _scale = value; _sizeChanged = true; } }

        protected virtual void SetColor(Color color)
        {
            if (Renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.color = color;
            else
                Debug.LogError($"SetColor: not a sprite {name}");
        }

        protected abstract void OnInitialize();
        protected abstract void OnDispose();
        protected abstract void OnGameObjectDestroyed();

        private void OnDestroy()
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
                foreach (var item in renderer.materials)
                    DestroyImmediate(item);

            OnGameObjectDestroyed();
        }

        protected abstract void UpdateLife();

        protected virtual void UpdatePosition()
        {
            var transform = gameObject.transform;
            var position = transform.localPosition;
            position.x = Position.x;
            position.y = Position.y;
            transform.localPosition = position;
            transform.localRotation = Quaternion.Euler(0, 0, Rotation);
        }

        protected virtual void UpdateSize()
        {
            gameObject.transform.localScale = Size * Scale * Vector3.one;
        }

        protected virtual void UpdateColor()
        {
            SetColor(new Color(Color.r, Color.g, Color.b, Color.a * Opacity * _alphaScale));
        }

        protected virtual void OnBeforeUpdate(float deltaTime) { }
        protected virtual void OnAfterUpdate(float deltaTime) { }

        public void Tick(float deltaTime)
        {
            if (!IsAlive)
                return;

            OnBeforeUpdate(deltaTime);

            if (IsAutomatic)
            {
                Position += Velocity * deltaTime;
                Rotation += AngularVelocity * deltaTime;
                Life -= deltaTime / Lifetime;

                if (Life <= 0)
                {
                    Dispose();
                    return;
                }
            }

            if (_lifeChanged)
            {
                UpdateLife();
                _lifeChanged = false;
            }

            if (_positionChanged)
            {
                UpdatePosition();
                _positionChanged = false;
            }

            if (_sizeChanged)
            {
                UpdateSize();
                _sizeChanged = false;
            }

            if (_colorChanged)
            {
                UpdateColor();
                _colorChanged = false;
            }

            OnAfterUpdate(deltaTime);
        }

        protected virtual Vector2 Velocity { get; private set; }
        protected virtual float AngularVelocity { get; private set; }

        protected float Lifetime { get; private set; }
        protected bool IsAutomatic { get; private set; }
    }
}
