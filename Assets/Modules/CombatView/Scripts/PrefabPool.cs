using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CombatView
{
    // Instances of the new combat prefabs (migration phase 5b, step 2), under the renderer root, so the combat scene
    // unload destroys them. Replaces the legacy GameObjectHolder and IObjectPool. Prefabs load from
    // Resources/CombatView/<path> with the legacy names (owner question 1); legacy prefabs are never loaded.
    public sealed class PrefabPool : IDisposable
    {
        public PrefabPool(Transform root)
        {
            var storage = new GameObject("Pool");
            storage.SetActive(false);
            storage.transform.SetParent(root, false);
            _storage = storage.transform;
        }

        // Null for a missing prefab; each missing path logs once.
        public GameObject Load(string path)
        {
            if (_prefabs.TryGetValue(path, out var prefab))
                return prefab;

            prefab = Resources.Load<GameObject>(ResourcePath + path);
            if (!prefab)
                Debug.LogWarning($"PrefabPool: no prefab {ResourcePath}{path}");
            _prefabs.Add(path, prefab);
            return prefab;
        }

        // A free instance, or a new one. The instance keeps its active state; its component initializes it.
        public GameObject Get(GameObject prefab, Transform parent)
        {
            if (_free.TryGetValue(prefab, out var free))
            {
                while (free.Count > 0)
                {
                    var instance = free.Pop();
                    if (!instance)
                        continue;

                    instance.transform.SetParent(parent, false);
                    _owners.Add(instance, prefab);
                    return instance;
                }
            }

            var created = Object.Instantiate(prefab, parent, false);
            _owners.Add(created, prefab);
            return created;
        }

        // Hides the instance until the next Get of its prefab. An instance from elsewhere is destroyed.
        public void Release(GameObject instance)
        {
            if (!instance)
                return;

            // The scene unload can destroy the storage first.
            if (!_owners.TryGetValue(instance, out var prefab) || !_storage)
            {
                Object.Destroy(instance);
                return;
            }

            _owners.Remove(instance);
            instance.SetActive(false);
            instance.transform.SetParent(_storage, false);
            if (!_free.TryGetValue(prefab, out var free))
                _free.Add(prefab, free = new Stack<GameObject>());
            free.Push(instance);
        }

        // The renderer root destroys the instances.
        public void Dispose()
        {
            _prefabs.Clear();
            _free.Clear();
            _owners.Clear();
        }

        private const string ResourcePath = "CombatView/";

        private readonly Transform _storage;
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<GameObject, Stack<GameObject>> _free = new Dictionary<GameObject, Stack<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> _owners = new Dictionary<GameObject, GameObject>();
    }
}
