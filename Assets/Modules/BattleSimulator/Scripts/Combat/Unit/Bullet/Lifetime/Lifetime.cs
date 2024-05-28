namespace Combat.Component.Bullet.Lifetime
{
    public class Lifetime : ILifetime
    {
        public Lifetime(float lifetime)
        {
            _lifetime = lifetime;
        }

        public void Restore(float amount)
        {
            if (amount <= 0 || _elapsed < amount)
                _elapsed = 0;
            else
                _elapsed -= amount;
        }

        public float Value { get { return _lifetime <= 0 ? 0 : _elapsed < _lifetime ? 1.0f - _elapsed/_lifetime : 0.0f; } }
        public bool IsExpired { get { return _elapsed > _lifetime; } }

        public void Take(float elapsedTime)
        {
            _elapsed += elapsedTime;
        }

        public float Max => _lifetime;
        public float Left => _elapsed >= _lifetime ? 0 : _lifetime - _elapsed;

        private float _elapsed;
        private readonly float _lifetime;
    }
}
