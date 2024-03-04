using UnityEngine;
using UnityEngine.UI;

namespace Gui.MainMenu
{
    [RequireComponent(typeof(RawImage))]
    public class BackgroundImage : MonoBehaviour
    {
        private RawImage _rawImage;

        public void SetImage(Texture2D texture)
        {
            _rawImage.texture = texture;
            OnRectTransformDimensionsChange();
        }

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rawImage == null) return;
            var texture = _rawImage.texture;
            if (texture == null) return;

            var aspect = (float)Screen.width / Screen.height;
            var textureAspect = (float)_rawImage.texture.width / _rawImage.texture.height;

            var rect = _rawImage.uvRect;

            if (textureAspect > aspect)
            {
                rect.width = aspect / textureAspect;
                rect.height = 1f;
                rect.x = (1f - rect.width)/2;
                rect.y = 0;
            }
            else
            {
                rect.width = 1f;
                rect.height = textureAspect/aspect;
                rect.x = 0f;
                rect.y = (1f - rect.height)/2;
            }

            _rawImage.uvRect = rect;
        }
    }
}
