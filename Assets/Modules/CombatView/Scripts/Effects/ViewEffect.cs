using CombatView.Views;
using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.ViewEffect (migration phase 5b, step 2): an effect drawn by a view component.
    // Changes: the pool replaces the GameObjectHolder, and Tick with the frame's time replaces Unity's Update.
    public class ViewEffect : MonoBehaviour, IEffectComponent
    {
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
                    if (value) gameObject.SetActive(true);
                }
            }
        }

        public Vector2 Position { get { return _position; } set { _position = value; _positionChanged = true; } }
        public float Rotation { get { return _rotation; } set { _rotation = value; _positionChanged = true; } }
        public Color Color { get { return _view.Color; } set { _view.Color = value; } }
        public float Life { get { return _view.Life; } set { _view.Life = value; } }
        public virtual float Size { get => _view.Size; set => _view.Size = value; }

        public virtual void Initialize(PrefabPool pool)
        {
            _view = GetComponent<IView>();
            _pool = pool;
            _isAutomatic = false;
            _lifetime = 1.0f;
            _velocity = Vector2.zero;
            _angularVelocity = 0;
            Position = Vector2.zero;
            Rotation = 0;
            IsAlive = true;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public bool IsAlive { get; private set; }

        public void Run(float lifetime, Vector2 velocity, float angularVelocity)
        {
            _isAutomatic = true;
            _lifetime = lifetime;
            _velocity = velocity;
            _angularVelocity = angularVelocity;

            Life = 1.0f;
        }

        public void Dispose()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            _pool.Release(gameObject);
        }

        public virtual void OnParentSizeChanged() { }
        public virtual void Restart() { }

        public void Tick(float deltaTime)
        {
            if (!IsAlive)
                return;

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
                UpdatePosition();
                UpdateRotation();
                _positionChanged = false;
            }

            UpdateView(deltaTime);
        }

        protected virtual void UpdateView(float deltaTime)
        {
            _view.UpdateView(deltaTime);
        }

        protected virtual void UpdatePosition()
        {
            gameObject.transform.localPosition = Position;
        }

        protected virtual void UpdateRotation()
        {
            gameObject.transform.localEulerAngles = new Vector3(0, 0, Rotation);
        }

        private bool _isAutomatic;
        private Vector2 _velocity;
        private float _angularVelocity;
        private float _lifetime;

        private Vector2 _position;
        private float _rotation;
        private bool _positionChanged;

        private IView _view;
        private PrefabPool _pool;
    }
}
