using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
	[RequireComponent(typeof(Camera))]
	public class DefaultCamera : MonoBehaviour
	{
		private Camera _camera;

		private void Awake()
		{
			_camera = GetComponent<Camera>();
		}

		private void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		private void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			UpdateCamera();
		}

		private void OnSceneUnloaded(Scene scene)
		{
			UpdateCamera();
		}

		private void UpdateCamera()
		{
			int count = Camera.allCamerasCount;

			if (count == 0)
				_camera.enabled = true;
			else if (count > 1)
				_camera.enabled = false;
		}
	}
}
