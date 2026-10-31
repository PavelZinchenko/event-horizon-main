using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    public interface IEffectPrefabInitializer
    {
        void Initialize(VisualEffectElement data, IResourceLocator resourceLocator);
    }

    // PrefabCache shares one element prefab per type and image, so values that differ
    // between elements go to each instance instead of the prefab (DB15.19).
    public interface IEffectInstanceInitializer
    {
        void Setup(VisualEffectElement data);
    }
}
