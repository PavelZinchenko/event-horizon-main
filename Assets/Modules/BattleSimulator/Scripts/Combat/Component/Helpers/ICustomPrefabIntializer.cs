using UnityEngine;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    public interface ICustomPrefabIntializer<T> where T : GameObjectPrefab
    {
        void Initialize(T data, IResourceLocator resourceLocator);
    }
}
