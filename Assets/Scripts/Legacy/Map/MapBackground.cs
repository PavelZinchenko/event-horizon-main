using UnityEngine;

public class MapBackground : MonoBehaviour 
{
	public Vector2 ScreenCenter = Vector2.zero;
	public float Scale = 1;
	public GameObject target;
	public int TextureScale = 5;
	public int DetailTextureScale = 20;
	public Material Material;

	[SerializeField] private Map.ScreenCenter _center;

	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private float _cameraAspect;

	private void Awake()
    {
		_meshRenderer = gameObject.AddComponent<MeshRenderer>();
		_meshRenderer.sharedMaterial = Material;
		_meshFilter = gameObject.AddComponent<MeshFilter>();
		UpdateMesh();
	}

	void LateUpdate() 
	{
		if (Material == null)
			return;

		UpdateMesh();

		var cameraSize = Camera.main.orthographicSize;

		UpdateTexture("_MainTex", TextureScale, cameraSize);
		UpdateTexture("_DecalTex", DetailTextureScale, cameraSize);

		transform.localScale = Vector3.one * cameraSize;
	}

	private void UpdateMesh()
    {
		var cameraAspect = Camera.main.aspect;
		if (Mathf.Abs(cameraAspect - _cameraAspect) < cameraAspect * 0.01f) return;

		_cameraAspect = cameraAspect;
		InitializeMesh(_meshFilter.mesh, 2 * _cameraAspect, 2 * _cameraAspect);
	}

	private void UpdateTexture(string name, int textureScale, float cameraSize)
	{
		var textureSize = 4*Mathf.Pow(cameraSize, 0.7f);

		if (_center)
			ScreenCenter = _center.NormalizedPosition;

		var x = -0.65f*(1-ScreenCenter.x)*cameraSize/(Scale*textureSize); // TODO
		var y = -0.5f*(1-ScreenCenter.y)*cameraSize/(Scale*textureSize);
		
		var targetPosition = _lastPosition;
		_lastPosition = target.transform.localPosition/Scale;
		var offset = 0.5f*(targetPosition - _lastPosition)/(textureSize*Camera.main.aspect);

        _textureOffset += offset;
		
		x += _textureOffset.x;
		y += _textureOffset.y;

		x *= textureScale;
		y *= textureScale;

		x = x - Mathf.FloorToInt(x) - 1;
		y = y - Mathf.FloorToInt(y) - 1;
		
		Material.SetTextureOffset(name, new Vector2(x, y));
		Material.SetTextureScale(name, Vector2.one * cameraSize * textureScale / textureSize);
	}

	private void InitializeMesh(Mesh mesh, float width, float height)
	{
		mesh.Clear(false);
		mesh.vertices = new Vector3[]
		{
			new Vector3 (-width/2, height/2, 0),
			new Vector3 (width/2, height/2, 0),
			new Vector3 (width/2, -height/2, 0),
			new Vector3 (-width/2, -height/2, 0)
		};
		mesh.triangles = new int[] { 0,1,2, 2,3,0 };
		mesh.uv = new Vector2[]
		{
			new Vector2 (0, 1/Scale), 
			new Vector2 (1/Scale, 1/Scale), 
			new Vector2 (1/Scale, 0), 
			new Vector2 (0, 0)
		};

		mesh.RecalculateNormals();
	}

	private Vector2 _textureOffset = Vector2.zero;
	private Vector2 _lastPosition = Vector2.zero;
}
