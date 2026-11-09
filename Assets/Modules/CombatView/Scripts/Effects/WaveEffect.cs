using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.WaveEffect (migration phase 5b, step 3): a growing ring. As a Shockwave element it
    // takes the element image and half the base size, as legacy WaveEffectInitializer did. Each instance builds its ring
    // and material once, not on every reuse.
    [RequireComponent(typeof(MeshFilter))]
    public class WaveEffect : EffectBase, IEffectElement
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private float _thickness = 1.0f;
        [SerializeField] private float _radius = 5.0f;
        [SerializeField] private int _segments = 32;
        [SerializeField] private float _baseSize = 1.0f;

        public void Configure(EffectKind kind, Sprite image)
        {
            _texture = image ? image.texture : null;
            _baseSize = ElementBaseSize;
        }

        public void Setup(EffectElement element) { }

        protected override void OnInitialize()
        {
            if (_mesh)
                return;

            _mesh = GetComponent<MeshFilter>().mesh;
            _material = Renderer.material;
            _material.mainTexture = _texture;
            var aspect = _texture ? (float)_texture.width / _texture.height : 1f;
            EffectMath.CreateCircle(_mesh, _radius, _thickness, _segments, aspect);
        }

        protected override void SetColor(Color color)
        {
            _material.color = color;
        }

        protected override void OnDispose() { }

        protected override void OnGameObjectDestroyed()
        {
            Destroy(_material);
            Destroy(_mesh);
        }

        protected override void UpdateLife()
        {
            Opacity = Life;
            Scale = (1f - Life) * _baseSize;
        }

        // Legacy WaveEffectInitializer.SetSize.
        private const float ElementBaseSize = 0.5f;

        private Mesh _mesh;
        private Material _material;
    }
}
