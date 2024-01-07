using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ShipEditor
{
	public class ShipLayoutElement : MonoBehaviour
	{
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] MeshRenderer _meshRenderer;
		[SerializeField] private Material _baseMaterial;

		private Mesh _mesh;
		private Material[] _materials;

		public void SetTextures(IEnumerable<Texture2D> textures)
		{
			DestroyMaterials();
			_materials = textures.Select(CreateMaterial).ToArray();
			_meshRenderer.sharedMaterials = _materials;
		}

		public void SetMesh(Mesh mesh)
		{
			DestroyMesh();
			_mesh = mesh;
			_meshFilter.sharedMesh = _mesh;
		}

		private void OnDestroy()
		{
			DestroyMesh();
			DestroyMaterials();
		}

		private Material CreateMaterial(Texture2D texture)
		{
			var material = new Material(_baseMaterial);
			material.mainTexture = texture;
			return material;
		}

		private void DestroyMesh()
		{
			if (_mesh == null) return;

			DestroyImmediate(_mesh);
			_mesh = null;
		}

		private void DestroyMaterials()
		{
			if (_materials == null) return;

			foreach (var material in _materials)
				DestroyImmediate(material);

			_materials = null;
		}
	}
}
