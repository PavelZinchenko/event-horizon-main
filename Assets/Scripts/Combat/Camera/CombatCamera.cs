using UnityEngine;
using Combat.Scene;
using GameServices.Settings;
using Zenject;

namespace Combat
{
	namespace Camera
	{
		[RequireComponent(typeof(UnityEngine.Camera))]
		public class CombatCamera : MonoBehaviour 
		{
			[SerializeField] GameObject Background;
			[SerializeField] float MinSize = 5;
            [SerializeField] float MaxSize = 25;
            //[SerializeField] float Border = 5;
			[SerializeField] float ZoomSpeed = 5;
			[SerializeField] float LinearSpeed = 5;
            [SerializeField] private float ZoomOverride = -1;

		    [Inject]
		    private void Initialize(IScene scene, GameSettings gameSettings)
		    {
		        _scene = scene;
		        _gameSettings = gameSettings;
		    }

            private void Start()
			{
                _zoom = ZoomOverride >= 0 ? ZoomOverride : _gameSettings.CameraZoom;
			}
			
			private void LateUpdate()
			{
				var camera = gameObject.GetComponent<UnityEngine.Camera>();
				var viewRect = _scene.ViewRect;
				var requestedZoom = CalculateExtendedZoom(_zoom);
				var orthographicSize = Mathf.Clamp(
					0.5f * Mathf.Max(viewRect.width / camera.aspect, viewRect.height),
					requestedZoom,
					MaxSize * 10f);
				var delta = Mathf.Min(ZoomSpeed*Time.unscaledDeltaTime, 1);
				camera.orthographicSize += (orthographicSize - camera.orthographicSize)*delta;
				var position = Vector2.Lerp(transform.position, viewRect.center, LinearSpeed*Time.unscaledDeltaTime);
				gameObject.Move(position);

				if (Background != null)
					Background.Move(position);
			}

			private float CalculateExtendedZoom(float value)
			{
				// Keep the original midpoint exactly unchanged while extending
				// both ends of the setting to ten times the original limits.
				const float midpoint = 0.5f;
				var originalMidZoom = Mathf.Lerp(MinSize, MaxSize, midpoint);
				if (value <= midpoint)
					return Mathf.Lerp(MinSize * 0.1f, originalMidZoom, Mathf.Clamp01(value / midpoint));

				return Mathf.Lerp(originalMidZoom, MaxSize * 10f,
					Mathf.Clamp01((value - midpoint) / midpoint));
			}

			private float _zoom;
            private IScene _scene;
		    private GameSettings _gameSettings;
		}
    }
}
