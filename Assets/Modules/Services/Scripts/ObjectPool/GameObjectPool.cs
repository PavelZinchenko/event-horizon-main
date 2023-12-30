using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Zenject;
using Object = UnityEngine.Object;

namespace Services.ObjectPool
{
	public sealed class GameObjectPool : IObjectPool, IDisposable, ITickable
	{
	    public GameObjectPool(GameObjectFactory factory)
	    {
	        _factory = factory;
	        _parent = _factory.CreateEmpty();
	    }

        public GameObject GetObject(GameObject prefab, bool injectDependencies = true)
        {
            if (!_parent)
            {
                throw new InvalidOperationException("GetObject " + prefab.name + ": GameObjectPool has been already destroyed");

                //Debug.LogError("GetObject " + prefab.name + ": GameObjectPool has been already destroyed");
                //return Object.Instantiate(prefab);
            }

            GameObject gameObject = null;
			if (_unusedObjectsCache.TryGetValue(prefab, out var objects))
			{
			    var needCleanup = false;
			    foreach (var item in objects)
			    {
			        if (item == null)
			        {
			            needCleanup = true;
                        continue;
			        }

			        gameObject = item;
                    objects.Remove(item);
                    break;
			    }

			    if (needCleanup)
			        objects.RemoveWhere(item => item == null);
			}

            if (gameObject == null)
                gameObject = injectDependencies ? _factory.Create(prefab) : Object.Instantiate(prefab, _parent.transform);
			if (gameObject == null)
				throw new System.ArgumentException();

			_objectPrefabs[gameObject] = prefab;
			gameObject.transform.parent = _parent.transform;
			gameObject.transform.parent = null;
			return gameObject;
		}

		public void PreloadObjects(GameObject prefab, int count, bool injectDependencies = true)
		{
		    if (!_parent)
		    {
		        Debug.Log("PreloadObjects " + prefab.name + ": GameObjectPool has been already destroyed");
                return;
		    }

			if (!_unusedObjectsCache.TryGetValue(prefab, out var objects)) 
			{
				objects = new HashSet<GameObject>();
				_unusedObjectsCache.Add(prefab, objects);
			}

			var available = objects.Count(item => item != null);

			for (var i = available; i < count; ++i)
			{
			    var gameObject = injectDependencies ? _factory.Create(prefab) : Object.Instantiate(prefab, _parent.transform);
				if (gameObject == null)
					throw new System.ArgumentException();

                _objectPrefabs[gameObject] = prefab;
				gameObject.transform.parent = _parent.transform;
				gameObject.SetActive(false);
				objects.Add(gameObject);
			}
		}

		public void ReleaseObject(GameObject gameObject)
		{
		    if (!_parent)
		    {
		        //Debug.Log("ReleaseObject " + gameObject.name + ": GameObjectPool has been already destroyed");
		        Object.Destroy(gameObject);
                return;
		    }

            if (!gameObject)
                return;

			gameObject.SetActive(false);
			gameObject.transform.parent = _parent.transform;

			if (!_objectPrefabs.TryGetValue(gameObject, out var prefab))
			{
				Object.Destroy(gameObject);
				return;
			}

			if (!_unusedObjectsCache.TryGetValue(prefab, out var objects))
			{
				objects = new HashSet<GameObject>();
				_unusedObjectsCache.Add(prefab, objects);
			}

			objects.Add(gameObject);
		}

		public void Dispose()
		{
			Object.Destroy(_parent);
			_parent = null;

			_objectPrefabs.Clear();

			foreach (var items in _unusedObjectsCache.Values)
			{
				foreach (var item in items)
				{
					item.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);
					Object.Destroy(item);
				}

				items.Clear();
			}

			_unusedObjectsCache.Clear();
		}

		private void RemoveUnusedPrefabs()
		{
			foreach (var key in _objectPrefabs.Keys.Where(item => !item).ToList())
			{
				_objectPrefabs.Remove(key);
			}
		}

		public void Tick()
		{
			var time = Time.realtimeSinceStartup;
			if (time - _lastUpdateTime > 10.0f)
			{
				_lastUpdateTime = time;
				RemoveUnusedPrefabs();
			}
		}

		private float _lastUpdateTime;
	    private GameObject _parent;
		private readonly Dictionary<GameObject, GameObject> _objectPrefabs = new Dictionary<GameObject, GameObject>();
		private readonly Dictionary<Object, HashSet<GameObject>> _unusedObjectsCache = new Dictionary<Object, HashSet<GameObject>>();

	    private readonly GameObjectFactory _factory;
    }

    public sealed class GameObjectFactory : IFactory<GameObject, GameObject>
    {
        [Inject] private readonly DiContainer _container;

        public GameObject CreateEmpty()
        {
            var gameObject = new GameObject("GameobjectPool");
            gameObject.transform.parent = _container.DefaultParent;
            return gameObject;
        }

        public GameObject Create(GameObject param)
        {
            var gameObject = _container.InstantiatePrefab(param);
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.SetParent(null, false);
            else
                gameObject.transform.parent = null;

            return gameObject;
        }
    }
}
