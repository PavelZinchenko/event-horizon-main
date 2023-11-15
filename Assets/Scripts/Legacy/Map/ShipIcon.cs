using UnityEngine;
using GameDatabase.DataModel;
using Services.Reources;

[RequireComponent(typeof(SpriteRenderer))]
public class ShipIcon : Square
{
	public float RotationSpeed = 30;
	public Color ColorHostile;
	public Color ColorNeutral;
	public Color ColorFriendly;

	private float _scale;
	private float _speed;
	private int _angle;
	private bool _isMapState;
	private float _distance;
	private SpriteRenderer _spriteRenderer;

	private void Awake()
    {
		_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Ship ship, IResourceLocator resourceLocator)
	{
		_spriteRenderer.sprite = resourceLocator.GetSprite(ship.ModelImage);
		_scale = Mathf.Sqrt(ship.ModelScale);
		_distance = 1 + 0.5f*_scale;
		_angle = ship.GetHashCode() % 360;
		_speed = 25f/ship.Layout.CellCount;
		transform.localScale = _scale * Size * Vector3.one;
	}

	private void SwitchToMapState(bool isMapState)
	{
		_isMapState = isMapState;
		if (_isMapState)
		{
			transform.localEulerAngles = new Vector3(0,0, 90);
			transform.localPosition = new Vector3(-_distance * Size, 0, transform.localPosition.z);
		}

		transform.localScale = Vector3.one * _scale * Size * (isMapState ? 1.5f : 1f);
	}
	
	void Update()
	{
		if (!_isMapState)
		{
			var angle = Time.time*RotationSpeed*_speed + _angle;
			transform.localEulerAngles = new Vector3(0,0, angle + 90);
			var position = _distance * Size * RotationHelpers.Direction(angle);
			transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
		}
	}
}
