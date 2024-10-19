using UnityEngine;

namespace Combat.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OutlineEffect : EffectBase
    {
        [SerializeField] private float _thickness = 0.1f;
        [SerializeField] private float _exponent = 0.33f;

        protected override void OnInitialize()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var sprite = transform.parent.GetComponent<SpriteRenderer>().sprite; // TODO: find better way
            renderer.sprite = sprite;

            var textureWidth = sprite.texture.width;
            var textureHeight = sprite.texture.height;
            var rect = sprite.textureRect;
            var spriteRect = new Vector4(rect.xMin / textureWidth, rect.xMax / textureWidth, rect.yMin / textureHeight, rect.yMax / textureHeight);
            renderer.material.SetVector("_Rect", spriteRect);
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void UpdateLife()
        {
            Opacity = Life;
        }

        protected override void UpdateSize()
        {
            Scale = CalculateThickness();
            base.UpdateSize();
            var renderer = GetComponent<SpriteRenderer>();
            renderer.material.SetFloat("_Scale", Scale);
        }

        private float CalculateThickness()
        {
            var parentScale = transform.parent.lossyScale.z;
            var scale = Mathf.Pow(parentScale, _exponent);
            return (scale + _thickness) / scale;
        }
    }
}
