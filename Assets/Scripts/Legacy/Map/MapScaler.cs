using GameServices.Player;
using Services.Messenger;
using Session;
using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Map
{
    [RequireComponent(typeof(Camera))]
    public class MapScaler : MonoBehaviour
    {
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly IMessenger _messenger;

        [SerializeField] private float _starZoomMin = 1f;
        [SerializeField] private float _starZoomMax = 2.5f;
        [SerializeField] private float _mapZoomMin = 5f;
        [SerializeField] private float _mapZoomMax = 25f;
        [SerializeField] private float _galaxyZoomMin = 100f;
        [SerializeField] private float _galaxyZoomMax = 150f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _zoomMinDelta = 0.05f;
        [SerializeField] private float _mapModeThreshold = 0.05f;

        [SerializeField] private UnityEvent _zoomChanged;

        private Camera _camera;

        private ViewMode _viewMode;
        private float _mapZoom;
        private float _starZoom;
        private float _galaxyZoom;

        private const float _allowance = 0.5f;

        public float CameraZoom => _camera.orthographicSize;

        public float ZoomMin => _starZoomMin;
        public float ZoomMax => _galaxyZoomMax;
		public ViewMode ViewMode => _viewMode;

		private void OnValidate()
        {
            if (_starZoomMax < _starZoomMin) _starZoomMax = _starZoomMin;
            if (_mapZoomMin < _starZoomMax) _mapZoomMin = _starZoomMax + 1f;
            if (_mapZoomMax < _mapZoomMin) _mapZoomMax = _mapZoomMin;
            if (_galaxyZoomMin < _mapZoomMax) _galaxyZoomMin = _mapZoomMax + 1f;
            if (_galaxyZoomMax < _galaxyZoomMin) _galaxyZoomMax = _galaxyZoomMin;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnDisable()
        {
            _session.StarMap.MapScaleFactor = _mapZoom;
            _session.StarMap.StarScaleFactor = _starZoom;
        }

        private void Start()
        {
            _mapZoom = Mathf.Clamp(_session.StarMap.MapScaleFactor, _mapZoomMin, _mapZoomMax);
            _starZoom = Mathf.Clamp(_session.StarMap.StarScaleFactor, _starZoomMin, _starZoomMax);
            _galaxyZoom = _galaxyZoomMin;

            _messenger.AddListener<ViewMode>(EventType.ViewModeChanged, OnViewModeChanged);
            OnViewModeChanged(_motherShip.ViewMode);

            _camera.orthographicSize = Zoom;
            _zoomChanged?.Invoke();
        }

        private void Update()
        {
            var cameraZoom = _camera.orthographicSize;
            if (Mathf.Abs(cameraZoom - Zoom) < _zoomMinDelta) return;

            var zoom = Mathf.Lerp(cameraZoom, Zoom, Time.deltaTime * _zoomSpeed);
            _camera.orthographicSize = zoom;
            _zoomChanged?.Invoke();

            UpdateViewMode();
        }

		public void ShowStarMap()
        {
            _viewMode = ViewMode.StarMap;
        }

        public void ShowStarSystem()
        {
            _viewMode = ViewMode.StarSystem;
        }

        public void ShowGalaxyMap()
        {
            _viewMode = ViewMode.GalaxyMap;
        }

        public void OnZoom(float scale)
        {
            Zoom = Mathf.Clamp(Zoom * scale, ZoomMin, ZoomMax);
            var force = scale - 1f;

            if (_viewMode == ViewMode.StarSystem)
            {
                Zoom = Mathf.Clamp(Zoom, _starZoomMin, _starZoomMax);
            }
            else if (_viewMode == ViewMode.StarMap)
            {
                if (Zoom >= _mapZoomMax && force > _mapModeThreshold)
                {
                    _viewMode = ViewMode.GalaxyMap;
                    Debug.LogWarning(_viewMode);
                }
                else
                    Zoom = Mathf.Clamp(Zoom, _mapZoomMin, _mapZoomMax);
            }
            else if (_viewMode == ViewMode.GalaxyMap)
            {
                if (Zoom <= _galaxyZoomMin && force < -_mapModeThreshold)
                {
                    _viewMode = ViewMode.StarMap;
                    Debug.LogWarning(_viewMode);
                }
                else
                    Zoom = Mathf.Clamp(Zoom, _galaxyZoomMin, _galaxyZoomMax);
            }
        }

        private void OnViewModeChanged(ViewMode viewMode)
        {
            _viewMode = viewMode;
            Debug.LogWarning("OnViewModeChanged - " + _viewMode);
        }

        private void UpdateViewMode()
        {
            if (_viewMode == _motherShip.ViewMode) return;

            var zoom = CameraZoom;

            var allowanceMin = 1f - _allowance;
            var allowanceMax = 1f + _allowance;

            if (_viewMode == ViewMode.StarSystem)
                if (zoom > _mapZoomMin * allowanceMax)
                    return;

            if (_viewMode == ViewMode.StarMap)
                if (zoom < _mapZoomMin * allowanceMin || zoom > _mapZoomMax * allowanceMax)
                    return;

            if (_viewMode == ViewMode.GalaxyMap)
                if (zoom < _mapZoomMax * allowanceMin)
                    return;

            _motherShip.ViewMode = _viewMode;
        }

        private float Zoom
        {
            get
            {
                switch (_viewMode)
                {
                    case ViewMode.StarMap:
                        return _mapZoom;
                    case ViewMode.StarSystem:
                        return _starZoom;
                    case ViewMode.GalaxyMap:
                        return _galaxyZoom;
                    default:
                        throw new InvalidOperationException();
                }
            }
            set
            {
                switch (_viewMode)
                {
                    case ViewMode.StarMap:
                        _mapZoom = Mathf.Clamp(value, _mapZoomMin, _mapZoomMax); ;
                        break;
                    case ViewMode.StarSystem:
                        _starZoom = Mathf.Clamp(value, _starZoomMin, _starZoomMax);
                        break;
                    case ViewMode.GalaxyMap:
                        _galaxyZoom = Mathf.Clamp(value, _galaxyZoomMin, _galaxyZoomMax);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
