using UnityEngine;

namespace Combat.Effects
{
    [RequireComponent(typeof(TextMesh))]
    public class TextEffect : EffectBase
    {
		[SerializeField] private bool _fading;

        protected override void OnInitialize()
        {
            _textMesh = GetComponent<TextMesh>();
            _textMesh.text = gameObject.name;
        }

        protected override void OnBeforeUpdate()
        {
			if (!Camera.main) return;
			var scale = Camera.main.orthographicSize;
			if (_fading) scale *= 1f - (1f - Life) * (1f - Life) * (1f - Life);
			Scale = scale;
        }

        protected override void SetColor(Color color)
        {
            _textMesh.color = color;
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}
        
		protected override void UpdateLife()
		{
			if (_fading)
				Opacity = 1f - (1f - Life) * (1f - Life);
		}

		private TextMesh _textMesh;
    }
}
