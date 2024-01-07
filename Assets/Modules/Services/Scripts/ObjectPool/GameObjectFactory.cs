using UnityEngine;
using Zenject;

namespace Services.ObjectPool
{
    public sealed class GameObjectFactory : IGameObjectFactory
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
