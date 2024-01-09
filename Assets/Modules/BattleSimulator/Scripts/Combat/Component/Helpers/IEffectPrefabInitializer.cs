using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    public interface IEffectPrefabInitializer
    {
        void Initialize(VisualEffectElement data, IResourceLocator resourceLocator);
    }
}
