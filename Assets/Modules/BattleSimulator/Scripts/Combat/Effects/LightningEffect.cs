using UnityEngine;
using Combat.Component.Helpers;
using GameDatabase.DataModel;
using Services.Resources;
using Combat.Component.View;
using Combat.Helpers;

namespace Combat.Effects
{
    [RequireComponent(typeof(LightningView), typeof(LineRenderer))]
    public class LightningEffect : ViewEffect, IEffectPrefabInitializer
    {
        [HideInInspector] [SerializeField] private LightningView _view;
        [HideInInspector] [SerializeField] private float _thickness;

        private Material _material;
        private float _direction;
        private bool _sizeChanged;

        public override float Size 
        {
            get => _view.Size;
            set
            {
                _view.Size = value;
                _sizeChanged = true;
            }
        }

        public void Initialize(VisualEffectElement data, IResourceLocator resourceLocator)
        {
            var renderer = GetComponent<LineRenderer>();
            _view = GetComponent<LightningView>();

            var sprite = resourceLocator.GetSprite(data.Image);
            if (_material) Destroy(_material);
            _material = renderer.material;
            _material.mainTexture = sprite == null ? null : sprite.texture;

            _view.Initialize(data.Color, data.ColorMode);
            _thickness = data.ParticleSize;
        }

        public override void Initialize(GameObjectHolder objectHolder)
        {
            base.Initialize(objectHolder);

            _direction = Random.Range(0, 360);
        }

        protected override void UpdateView()
        {
            base.UpdateView();

            if (_sizeChanged)
            {
                _sizeChanged = false;
                _view.Thickness = _thickness * transform.lossyScale.z;
                _view.UpdateLine();
            }
        }

        protected override void UpdateRotation()
        {
            gameObject.transform.localEulerAngles = new Vector3(0, 0, Rotation + _direction);
        }
    }
}
