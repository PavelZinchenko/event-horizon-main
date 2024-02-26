using UnityEngine;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.Helpers
{
    class WormTailSegmentInitializer : MonoBehaviour, ICustomPrefabIntializer<GameObjectPrefab_WormTailSegment>, IWormTailConfiguration
    {
        [SerializeField] private SpriteRenderer _bodySprite;
        [SerializeField] private SpriteRenderer _jointSprite;
        [SerializeField] private SpriteRenderer _bodyWreckSprite;
        [SerializeField] private SpriteRenderer _jointWreckSprite;

        [SerializeField] private float _segmentLength = 0.45f;
        [SerializeField] private float _jointOffset = 0.35f;
        [SerializeField] private float _maxRotation = 30f;
        [SerializeField] private float _maxHeadRotation = 60f;
        [SerializeField] private float _headOffset = 0.15f;

        public float SegmentLength => _segmentLength;
        public float JointOffset => _jointOffset;
        public float MaxRotation => _maxRotation;
        public float MaxHeadRotation => _maxHeadRotation;
        public float HeadOffset => _headOffset;

        public void Initialize(GameObjectPrefab_WormTailSegment data, IResourceLocator resourceLocator)
        {
            var bodySprite = resourceLocator.GetSprite(data.BodyImage);
            var jointSprite = resourceLocator.GetSprite(data.JointImage);

            _bodySprite.sprite = bodySprite;
            _bodyWreckSprite.sprite = bodySprite;
            _jointSprite.sprite = jointSprite;
            _jointWreckSprite.sprite = jointSprite;

            _segmentLength = data.BoneLength;
            _jointOffset = data.JointOffset;
            _maxRotation = data.MaxRotation;
            _maxHeadRotation = data.MaxHeadRotation;
            _headOffset = data.HeadOffset;

            _jointSprite.transform.localScale = data.JointImageScale * Vector3.one;
            _jointSprite.transform.localPosition = new Vector3(data.JointImageOffset, 0, 0);
            _jointWreckSprite.transform.localScale = data.JointImageScale * Vector3.one;
            _jointWreckSprite.transform.localPosition = new Vector3(data.JointImageOffset, 0, 0);
        }
    }

    public interface IWormTailConfiguration
    {
        float HeadOffset { get; }
        float JointOffset { get; }
        float SegmentLength { get; }
        float MaxRotation { get; }
        float MaxHeadRotation { get; }
    }
}
