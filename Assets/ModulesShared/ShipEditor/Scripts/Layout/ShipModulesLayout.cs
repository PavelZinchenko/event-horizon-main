using System.Collections.Generic;
using UnityEngine;
using Services.Resources;
using Zenject;
using ShipEditor.Model;

namespace ShipEditor
{
	public class ShipModulesLayout : MonoBehaviour
	{
		[Inject] private readonly IResourceLocator _resourceLocator;

		[SerializeField] private ShipLayoutElement _modulePrefab;

		private Dictionary<Texture2D, ModuleGroup> _groups = new();
		private float _cellSize;
        private int _x0;
        private int _y0;

		public void Initialize(float cellSize, int x0, int y0)
		{
			Cleanup();
			_cellSize = cellSize;
            _x0 = x0;
            _y0 = y0;
		}

		public void AddComponent(IComponentModel component, bool updateImmediately = true)
		{
			var data = component.Data;
			var texture = _resourceLocator.GetSprite(data.Icon)?.texture;
			if (texture == null) return;

			if (!_groups.TryGetValue(texture, out var group))
			{
				var builder = new ModuleMeshBuilder(_resourceLocator, _cellSize);
				var module = Instantiate(_modulePrefab, transform);
				group = new ModuleGroup { Texture = texture, Builder = builder, Layout = module };
				_groups.Add(texture, group);
			}

			group.Builder.AddComponent(component.X - _x0, component.Y - _y0, data, component.Layout, component.Rotation);
			
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
