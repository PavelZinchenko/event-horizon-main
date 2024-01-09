using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    public interface IBulletPrefabInitializer
    {
        void Initialize(BulletPrefab data, IResourceLocator resourceLocator);
    }
}
