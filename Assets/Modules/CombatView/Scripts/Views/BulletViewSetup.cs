using CombatView.Effects;
using UnityEngine;

namespace CombatView.Views
{
    // The root component of the bullet prefabs (migration phase 5b, step 5). It replaces the legacy
    // ProjectileInitializer, BeamInitializer and ParticleSystemInitializer, and the initializer part of
    // CircularSawView. Configure runs once on the template of a look, as legacy PrefabCache.GetBulletPrefab ran the
    // initializer; the instances copy its serialized values. Bind runs on each instance.
    public class BulletViewSetup : MonoBehaviour
    {
        [SerializeField] private SpriteView _bulletView;
        [SerializeField] private SpriteView _engineView;
        [SerializeField] private TrailView _trailView;
        [SerializeField] private Transform _bulletObject;
        [SerializeField] private Transform _engineObject;
        [SerializeField] private Transform _trailObject;
        [SerializeField] private RopeView _rope;
        [SerializeField] private LineRenderer _line;
        [SerializeField] private LaserView _laserView;
        [SerializeField] private LightningView _lightningView;
        [SerializeField] private ArcView _arcView;
        [SerializeField] private ParticleSystemView _particleView;
        [SerializeField] private CircularSawView _sawView;
        [SerializeField] private Texture _lineTexture;

        // The root view: the renderer sets its life, colour and size.
        public BaseView View => _view ? _view : _view = GetComponent<BaseView>();

        // The harpoon rope; null for the other shapes.
        public RopeView Rope => _rope;

        public void Configure(BulletLook look)
        {
            // Legacy ProjectileInitializer.
            if (_bulletView)
            {
                _bulletView.Initialize(look.Image, look.MainColor, look.MainColorMode);
                _bulletView.SetMargins(look.Margins);
            }

            if (_bulletObject) _bulletObject.localScale = Vector3.one * look.Size;

            if (_engineView) _engineView.Initialize(null, look.SecondColor, look.SecondColorMode);
            if (_engineObject) _engineObject.localPosition = new Vector3(-look.Margins, 0, 0);

            if (_trailView) _trailView.Initialize(look.SecondColor, look.SecondColorMode);
            if (_trailObject) _trailObject.localPosition = new Vector3(-look.Margins - 0.1f, 0, 0);

            if (_rope) _rope.Color = look.SecondColor;

            // Legacy BeamInitializer. The image goes to each instance through a property block, so the instances share
            // the prefab material.
            if (_line) _lineTexture = look.Image ? look.Image.texture : null;

            if (_laserView)
            {
                _laserView.Initialize(look.MainColor, look.MainColorMode);
                _laserView.BorderSize = look.Margins;
                _laserView.Thickness = look.Size;
            }
            else if (_lightningView)
            {
                _lightningView.Initialize(look.MainColor, look.MainColorMode);
                _lightningView.Thickness = look.Size;
            }
            else if (_arcView)
            {
                _arcView.Initialize(look.MainColor, look.MainColorMode, look.Margins);
            }

            // Legacy ParticleSystemInitializer: the root scale becomes the view's initial size.
            if (_particleView)
            {
                _particleView.Initialize(look.MainColor, look.MainColorMode);
                _particleView.transform.localScale = Vector3.one * look.Size;
            }

            // Legacy CircularSawView.Initialize.
            if (_sawView) _sawView.Initialize(look.Image, look.MainColor, look.MainColorMode);
        }

        // The trail pool is not serialized, so each instance gets it. Null draws no trail.
        public void Bind(TrailPool trails)
        {
            if (_trailView) _trailView.Pool = trails;
        }

        private void Awake()
        {
            if (_line) EffectMath.SetTexture(_line, _lineTexture, new MaterialPropertyBlock());
        }

        private BaseView _view;
    }
}
