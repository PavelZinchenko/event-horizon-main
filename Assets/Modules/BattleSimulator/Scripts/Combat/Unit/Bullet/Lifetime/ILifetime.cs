namespace Combat.Component.Bullet.Lifetime
{
    public interface ILifetime
    {
        float Max { get; }

        void Restore(float amount = 0);
        float Value { get; }
        bool IsExpired { get; }

        void Take(float elapsedTime);
    }
}
