using Galaxy;
using GameServices.Player;
using Services.Audio;
using Services.Messenger;
using UnityEngine;
using Zenject;

public class PlayerShipObject : MonoBehaviour
{
    [Inject] private readonly ISoundPlayer _soundPlayer;
    [Inject] private readonly MotherShip _motherShip;
    [Inject] private readonly StarData _starData;

	[SerializeField] private SpriteRenderer _shipIcon;
	[SerializeField] private float _shipOrbitRadius = 1.5f;
	[SerializeField] private float _distanceBetweenStars = 8.0f;
	[SerializeField] private float _shipGalaxyModeSize = 9.0f;
	[SerializeField] private float _shipMapModeSize = 1.5f;
	[SerializeField] private AudioClip _engineSound;

	private bool _isMoving;
	private int _startId;
	private int _endId;
	private bool _isPlaying;

	private bool _galaxyMapMode = false;
	private Vector2 _start;
	private Vector2 _control;
	private Vector2 _end;
	private float _shipOrbitalAngle;

	[Inject]
    private void Initialize(IMessenger messenger)
    {
        messenger.AddListener<int, int, float>(EventType.PlayerShipMoved, OnShipMoved);
        messenger.AddListener<int>(EventType.PlayerPositionChanged, OnPositionChanged);
        messenger.AddListener<ViewMode>(EventType.ViewModeChanged, OnMapStateChanged);

        gameObject.Move(_motherShip.CurrentStar.Position * _distanceBetweenStars);
		OnMapStateChanged(_motherShip.ViewMode);
	}

	public void PlaySound()
    {
		if (_isPlaying) return;
		_soundPlayer.Play(_engineSound, GetHashCode(), true);
		_isPlaying = true;
	}

	public void StopSound()
    {
		if (!_isPlaying) return;
		_soundPlayer.Stop(GetHashCode());
		_isPlaying = false;
	}

	private void Start()
    {
		UpdatePosition();
	}

	private void Update()
	{
		UpdatePosition();
	}

	public event System.Action<Vector2> MovedEvent = position => {};

	private void OnPositionChanged(int starId)
	{
		_startId = _endId = -1;
		_isMoving = false;
		StopSound();

		gameObject.Move(_starData.GetPosition(starId) * _distanceBetweenStars);
	}

	private void OnShipMoved(int source, int target, float progress)
	{
		PlaySound();
		_isMoving = true;

		if (_startId != source || _endId != target)
		{
			_startId = source;
			_endId = target;

			var targetPosition = _starData.GetPosition(_endId) * _distanceBetweenStars;

			_start = (Vector2)_shipIcon.transform.localPosition + ((Vector2)transform.localPosition - targetPosition);
			_end = _shipOrbitRadius * _start.normalized;
			_shipOrbitalAngle = Mathf.Atan2(_start.x, _start.y);
			_control = _start + 0.5f*Mathf.Min(Vector2.Distance(_end,_start), _distanceBetweenStars)*RotationHelpers.Direction(_shipIcon.transform.localEulerAngles.z);

			gameObject.Move(targetPosition);
			_shipIcon.transform.localPosition = _start;
		}

		var current = Geometry.Bezier(_start, _control, _end, progress);
		var dir = current - (Vector2)_shipIcon.transform.localPosition;
		_shipIcon.transform.localPosition = current;
		if (dir.sqrMagnitude > float.Epsilon)
			_shipIcon.transform.localEulerAngles = new Vector3(0,0,RotationHelpers.Angle(dir));

		MovedEvent((Vector2)transform.localPosition + current);
	}

	private void UpdatePosition()
    {
		if (_isMoving || _galaxyMapMode)
			return;

		var delta = Time.deltaTime;
		_shipOrbitalAngle += delta / 20;
		_shipIcon.transform.localPosition = new Vector2(_shipOrbitRadius * Mathf.Sin(_shipOrbitalAngle), _shipOrbitRadius * Mathf.Cos(_shipOrbitalAngle));
		_shipIcon.transform.localEulerAngles = new Vector3(0, 0, _shipIcon.transform.localEulerAngles.z + Mathf.Rad2Deg * delta / 12);
	}

	private void OnMapStateChanged(ViewMode state)
	{
		_galaxyMapMode = state == ViewMode.GalaxyMap;

		if (_galaxyMapMode)
		{
			_shipIcon.transform.localPosition = Vector2.zero;
			_shipIcon.transform.localEulerAngles = new Vector3(0,0,90);
			_shipIcon.transform.localScale = Vector3.one * _shipGalaxyModeSize;
		}
		else
		{
			_shipIcon.transform.localScale = Vector3.one * _shipMapModeSize;
		}
	}
}
