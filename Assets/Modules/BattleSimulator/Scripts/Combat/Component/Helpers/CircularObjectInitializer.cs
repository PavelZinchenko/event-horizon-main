using UnityEngine;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    class CircularObjectInitializer : MonoBehaviour, ICustomPrefabIntializer<GameObjectPrefab_CircularSpriteObject>
    {
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private CircleCollider2D _collider;

        public void Initialize(GameObjectPrefab_CircularSpriteObject data, IResourceLocator resourceLocator)
        {
            var sprite = resourceLocator.GetSprite(data.Image);
            _sprite.sprite = sprite;
            _collider.radius = 0.5f/data.ImageScale;
        }
    }
}
