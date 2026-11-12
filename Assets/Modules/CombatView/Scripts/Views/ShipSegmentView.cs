using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.ShipSegmentView (migration phase 5b, step 7b): the view of the worm tail
    // segment prefabs. HsvMaterials replaces the MaterialCache. The renderer references of legacy
    // WormTailSegmentInitializer moved here; Initialize does its work for a database segment prefab.
    public sealed class ShipSegmentView : BaseView
    {
        [SerializeField] private GameObject _normalView;
        [SerializeField] private GameObject _wreckView;

        [SerializeField] private SpriteRenderer[] _renderers;

        [SerializeField] private SpriteRenderer _bodySprite;
        [SerializeField] private SpriteRenderer _jointSprite;
        [SerializeField] private SpriteRenderer _bodyWreckSprite;
        [SerializeField] private SpriteRenderer _jointWreckSprite;

        private Material _defaultMaterial;

        // Legacy WormTailSegmentInitializer.Initialize, without the joint values the core already has.
        public void Initialize(Sprite body, Sprite joint, float jointScale, float jointOffset)
        {
            if (body)
            {
                _bodySprite.sprite = body;
                _bodyWreckSprite.sprite = body;
            }

            if (joint)
            {
                _jointSprite.sprite = joint;
                _jointWreckSprite.sprite = joint;
            }

            _jointSprite.transform.localScale = jointScale * Vector3.one;
            _jointSprite.transform.localPosition = new Vector3(jointOffset, 0, 0);
            _jointWreckSprite.transform.localScale = jointScale * Vector3.one;
            _jointWreckSprite.transform.localPosition = new Vector3(jointOffset, 0, 0);
        }

        protected override void OnGameObjectCreated()
        {
            _activeView = _normalView;
            _normalView.SetActive(true);
            _wreckView.SetActive(false);
        }

        public void ApplyHsv(float hue, float saturation, HsvMaterials materialCache)
        {
            _defaultMaterial = materialCache.GetDefaultMaterial();
            var material = materialCache.GetHsvMaterial(hue, saturation);

            if (_renderers != null && _renderers.Length > 0)
                foreach (var item in _renderers)
                    item.sharedMaterial = material;
        }

        public override void Dispose()
        {
            if (this && _defaultMaterial && _renderers != null && _renderers.Length > 0)
                foreach (var item in _renderers)
                    item.sharedMaterial = _defaultMaterial;
        }

        protected override void OnGameObjectDestroyed()
        {
        }

        protected override void UpdateLife(float life)
        {
            if (life > 0 && _activeView != _normalView)
            {
                _wreckView.SetActive(false);
                _normalView.SetActive(true);
                _activeView = _normalView;
            }

            if (life <= 0 && _activeView != _wreckView)
            {
                _wreckView.SetActive(true);
                _normalView.SetActive(false);
                _activeView = _wreckView;
            }
        }

        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}
        protected override void UpdateSize(float size) {}
        protected override void UpdateColor(Color color) {}

        private GameObject _activeView;
    }
}
