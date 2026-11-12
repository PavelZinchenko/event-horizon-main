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

        public GameObject Create(GameObjectPrefab_Undefined content) => throw new System.InvalidOperationException();
        public GameObject Create(GameObjectPrefab_WormTailSegment content) => LoadPrefab(content, "WormSegment");
        public GameObject Create(GameObjectPrefab_CircularSpriteObject content) => LoadPrefab(content, "EnergyShield");
        public GameObject Create(GameObjectPrefab_CircularOutlineObject content) => LoadPrefab(content, "EnergyShieldOutline");

        // Migration phase 5b, step 7c: the new combat scene's frontal shield record. Legacy draws its frontal shield from
        // the FrontalShield prefab and never loads the record; another device that names it gets the prefab as it is.
        public GameObject Create(GameObjectPrefab_ColliderSpriteObject content)
        {
            var prefab = GameObject.Instantiate(_prefabCache.LoadPrefab(new PrefabId("FrontalShield", PrefabId.Type.Object)));
            prefab.transform.parent = _prefabCache.transform;
            return prefab;
        }

        private GameObject LoadPrefab<T>(T content, string name) where T : GameObjectPrefab
        {
            var template = _prefabCache.LoadPrefab(new PrefabId(name, PrefabId.Type.Object));
            var prefab = GameObject.Instantiate(template);
            prefab.GetComponent<ICustomPrefabIntializer<T>>().Initialize(content, _resourceLocator);
            prefab.transform.parent = _prefabCache.transform;
            return prefab;
        }
    }
}
