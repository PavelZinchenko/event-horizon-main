using UnityEngine;
using Zenject;

namespace Services.ObjectPool
{
    public interface IGameObjectFactory : IFactory<GameObject, GameObject>
    {
        public GameObject CreateEmpty();
        //public GameObject Create(GameObject param);
    }
}
