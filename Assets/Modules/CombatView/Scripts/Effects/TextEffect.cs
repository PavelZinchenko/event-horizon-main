using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.TextEffect (migration phase 5b, step 4): a text that keeps its screen size while
    // the camera zooms. Text replaces the legacy GameObjectHolder name as the source of the text.
    [RequireComponent(typeof(TextMesh))]
    public class TextEffect : EffectBase
    {
		[SerializeField] private bool _fading;

        public string Text
        {
            set => TextMesh.text = value;
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnBeforeUpdate(float deltaTime)
        {
			if (!Camera.main) return;
			var scale = Camera.main.orthographicSize;
			if (_fading) scale *= 1f - (1f - Life) * (1f - Life) * (1f - Life);
			Scale = scale;
        }

        protected override void SetColor(Color color)
        {
            TextMesh.color = color;
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

		protected override void UpdateLife()
		{
			if (_fading)
				Opacity = 1f - (1f - Life) * (1f - Life);
		}

        private TextMesh TextMesh => _textMesh ? _textMesh : _textMesh = GetComponent<TextMesh>();

		private TextMesh _textMesh;
    }
}
