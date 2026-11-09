using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.IEffect (migration phase 5b, step 2). EffectPool drives every effect through
    // Tick, so no effect updates itself in a Unity Update.
    public interface IEffect
    {
        bool Visible { get; set; }
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        float Size { get; set; }
        Color Color { get; set; }
        float Life { get; set; }

        void OnParentSizeChanged();
        void Restart();

        bool IsAlive { get; }
        void Run(float lifetime, Vector2 velocity, float angularVelocity);
        void Tick(float deltaTime);
        void Dispose();
    }

    // The component on the root of an effect prefab. Legacy took a GameObjectHolder; Dispose now gives the instance
    // back to the pool.
    public interface IEffectComponent : IEffect
    {
        void Initialize(PrefabPool pool);
    }
}
