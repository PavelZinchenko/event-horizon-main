using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour
{
    public float Size = 1.0f;
    public Color Color = Color.white;
    public Texture2D Texture;
    public Material Material;

    public void SetColor(Color color)
    {
        if (_renderer == null)
            _renderer = gameObject.GetComponent<Renderer>();
        if (_renderer != null)
            _renderer.material.color = color;
    }

    public void SetTexture(Texture2D texture)
    {
        Texture = texture != null ? texture : _defaultTexture;
        if (_renderer == null)
            _renderer = gameObject.GetComponent<Renderer>();
        if (_renderer != null)
            _renderer.material.mainTexture = Texture;
    }

    void Awake()
    {
        _defaultTexture = Texture;
        gameObject.transform.localScale = Vector3.one * Size;
        gameObject.CreateMeshFilter().sharedMesh = SharedResources.Instance.SquareMesh;
        if (Material != null)
            gameObject.CreateMaterial(Material, Texture, Color);
        else
            gameObject.CreateDefaultMaterial(Texture, Color);
    }

    private Renderer _renderer;
    private Texture2D _defaultTexture;
}
