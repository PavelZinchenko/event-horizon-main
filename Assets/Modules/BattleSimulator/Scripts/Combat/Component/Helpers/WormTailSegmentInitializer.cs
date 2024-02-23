using UnityEngine;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    class WormTailSegmentInitializer : MonoBehaviour, ICustomPrefabIntializer<GameObjectPrefab_WormTailSegment>
    {
        [SerializeField] private SpriteRenderer _bodySprite;
        [SerializeField] private SpriteRenderer _jointSprite;
        [SerializeField] private SpriteRenderer _bodyWreckSprite;
        [SerializeField] private SpriteRenderer _jointWreckSprite;

        public void Initialize(GameObjectPrefab_WormTailSegment data, IResourceLocator resourceLocator)
        {
            var bodySprite = resourceLocator.GetSprite(data.BodyImage);
            var jointSprite = resourceLocator.GetSprite(data.JointImage);

            _bodySprite.sprite = bodySprite;
            _bodyWreckSprite.sprite = bodySprite;
            _jointSprite.sprite = jointSprite;
            _jointWreckSprite.sprite = jointSprite;
        }
    }
}
