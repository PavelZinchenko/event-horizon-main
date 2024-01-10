using System.Collections.Generic;
using UnityEngine;
using Services.Resources;
using Zenject;

namespace ShipEditor
{
	public class ShipModulesLayout : MonoBehaviour
	{
		[Inject] private readonly IResourceLocator _resourceLocator;

		[SerializeField] private ShipLayoutElement _modulePrefab;

		private Dictionary<Texture2D, ModuleGroup> _groups = new();
		private float _cellSize;

		public void Initialize(float cellSize)
		{
			Cleanup();
			_cellSize = cellSize;
		}

		public void AddComponent(int x, int y, GameDatabase.DataModel.Component component, bool updateImmediately = true)
		{
			var texture = _resourceLocator.GetSprite(component.Icon)?.texture;
			if (texture == null) return;

			if (!_groups.TryGetValue(texture, out var group))
			{
				var builder = new ModuleMeshBuilder(_resourceLocator, _cellSize);
				var module = Instantiate(_modulePrefab, transform);
				group = new ModuleGroup { Texture = texture, Builder = builder, Layout = module };
				_groups.Add(texture, group);
			}

			group.Builder.AddComponent(x, y, component);
			
			if (updateImmediately) 
				group.Update();
		}

		public void UpdateMesh()
		{
			foreach (var group in _groups.Values)
				group.Update();
		}

		private void Cleanup()
		{
			foreach (var group in _groups.Values)
				Destroy(group.Layout);

			_groups.Clear();
		}

		private struct ModuleGroup
		{
			public Texture2D Texture;
			public ModuleMeshBuilder Builder;
			public ShipLayoutElement Layout;

			public void Update()
			{
				Layout.SetMesh(Builder.CreateMesh());
				Layout.SetTextures(Texture.ToEnumerable());
			}
		}
	}
}
