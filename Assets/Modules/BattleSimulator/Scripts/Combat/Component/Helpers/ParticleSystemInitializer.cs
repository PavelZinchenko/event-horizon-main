using Combat.Component.View;
using GameDatabase.DataModel;
using Services.Resources;
using UnityEngine;

namespace Combat.Component.Helpers
{
    public class ParticleSystemInitializer : MonoBehaviour, IBulletPrefabInitializer
    {
        [SerializeField] private ParticleSystemView View;

        public void Initialize(BulletPrefab data, IResourceLocator resourceLocator)
        {
            View.Initialize(data.MainColor, data.MainColorMode);
            View.transform.localScale = Vector3.one * data.Size;
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }

        private Material _material;
    }
}
