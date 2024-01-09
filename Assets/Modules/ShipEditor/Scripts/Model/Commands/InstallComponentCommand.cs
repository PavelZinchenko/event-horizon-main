using Constructor;

namespace ShipEditor.Model
{
	public class InstallComponentCommand : ICommand
	{
		private readonly UnityEngine.Vector2Int _position;
		private readonly ComponentInfo _component;
		private readonly ComponentSettings _settings;
		private readonly ShipElementType _shipElement;
		private readonly IShipEditorModel _shipEditor;

		public bool BelongsToElement(ShipElementType shipElement) => shipElement == _shipElement;

		public InstallComponentCommand(
			IShipEditorModel shipEditor,
			ShipElementType shipElement,
			UnityEngine.Vector2Int position,
			ComponentInfo component,
			ComponentSettings settings)
		{
			_position = position;
			_component = component;
			_settings = settings;
			_shipElement = shipElement;
			_shipEditor = shipEditor;
		}

		public bool TryExecute()
		{
			return _shipEditor.TryInstallComponent(_shipElement, _position, _component, _settings);
		}

		public bool TryRollback()
		{
			if (!_shipEditor.TryFindComponent(_shipElement, _position, _component, out var component)) return false;
			_shipEditor.RemoveComponent(component);
			return true;
		}
	}
}
