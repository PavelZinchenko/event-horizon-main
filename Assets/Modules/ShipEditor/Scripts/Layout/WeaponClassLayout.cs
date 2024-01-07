using UnityEngine;
using GameDatabase.Model;

namespace ShipEditor
{
	public class WeaponClassLayout : MonoBehaviour
	{
		[SerializeField] private Sprite[] _letters;
		[SerializeField] private ShipLayoutElement _weaponClasses;
		[SerializeField] private Vector2 _letterSizeMax = new Vector2(0.3f, 0.5f);
		[SerializeField] private Color _color;

		private WeaponClassesMeshBuilder _builder;

		public void Initialize(float cellSize, ShipMeshBuilder.ILayout layout)
		{
			Cleanup();

			_builder = new WeaponClassesMeshBuilder(cellSize, _letterSizeMax, layout);
			_builder.Color = _color;

			for (int i = 0; i < _letters.Length; ++i)
				_builder.AddLetter((char)('A' + i), _letters[i]);

			Build();
		}

		public void AddComponent(int x, int y, Layout layout)
		{
			if (_builder.TryAddElement(layout, x, y))
				_weaponClasses.SetMesh(_builder.CreateMesh());
		}

		public void RemoveComponent(int x, int y, Layout layout)
		{
			if (_builder.TryRemoveElement(layout, x, y))
				_weaponClasses.SetMesh(_builder.CreateMesh());
		}

		public void Cleanup()
		{
			_weaponClasses.SetMesh(null);
		}

		private void Build()
		{
			_weaponClasses.SetMesh(_builder.CreateMesh());
		}
	}
}
