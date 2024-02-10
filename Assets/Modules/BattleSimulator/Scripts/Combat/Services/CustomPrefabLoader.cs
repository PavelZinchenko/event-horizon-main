using Combat.Component.Helpers;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Services.Resources;
using UnityEngine;

namespace Combat.Services
{
    public class CustomPrefabLoader : IGameObjectPrefabFactory<GameObject>
    {
        private readonly PrefabCache _prefabCache;
        private readonly IResourceLocator _resourceLocator;

        public CustomPrefabLoader(PrefabCache prefabCache, IResourceLocator resourceLocator)
        {
            _prefabCache = prefabCache;
            _resourceLocator = resourceLocator;
        }

        public GameObject Create(GameObjectPrefab_Undefined content)
        {
            throw new System.InvalidOperationException();
        }

        public GameObject Create(GameObjectPrefab_WormTailSegment content)
        {
            var template = _prefabCache.LoadPrefab(new PrefabId("WormSegment", PrefabId.Type.Object));
            var prefab = GameObject.Instantiate(template);
            prefab.GetComponent<WormTailSegmentInitializer>().Initialize(content, _resourceLocator);
            return prefab;
        }
    }
}
