using UnityEngine;

namespace Combat.Component.View
{
    public sealed class AllyShipMarker : MonoBehaviour
    {
        private void Awake()
        {
            _marker = new GameObject("Preview9AllyTriangle");
            var filter = _marker.AddComponent<MeshFilter>();
            var renderer = _marker.AddComponent<MeshRenderer>();
            var mesh = new Mesh { name = "Preview9AllyTriangleMesh" };
            mesh.vertices = new[]
            {
                new Vector3(-1.25f, 0.9f, 0f),
                new Vector3(1.25f, 0.9f, 0f),
                new Vector3(0f, -1.25f, 0f)
            };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;
            var material = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.03f, 0.48f, 1f, 1f)
            };
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 120;
            _mesh = mesh;
            _material = material;
        }

        private void LateUpdate()
        {
            if (_marker == null)
                return;

            _marker.transform.position = transform.position + Vector3.up * 0.35f;
            _marker.transform.rotation = Quaternion.identity;
            _marker.transform.localScale = Vector3.one * 0.72f;
        }

        private void OnDisable()
        {
            if (_marker != null)
                _marker.SetActive(false);
        }

        private void OnEnable()
        {
            if (_marker != null)
                _marker.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_marker != null)
                Destroy(_marker);
            if (_mesh != null)
                Destroy(_mesh);
            if (_material != null)
                Destroy(_material);
        }

        private GameObject _marker;
        private Mesh _mesh;
        private Material _material;
    }
}
