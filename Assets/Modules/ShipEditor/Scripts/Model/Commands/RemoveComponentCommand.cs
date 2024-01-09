namespace ShipEditor.Model
{
	public class RemoveComponentCommand : ICommand
	{
		private readonly IShipEditorModel _shipEditor;
		private readonly IComponentModel _component;

		public bool BelongsToElement(ShipElementType shipElement) => shipElement == _component.Location;

		public RemoveComponentCommand(IShipEditorModel shipEditor, IComponentModel component)
		{
			_shipEditor = shipEditor;
			_component = component;
		}

		public bool TryExecute()
		{
			_shipEditor.RemoveComponent(_component);
			return true;
		}

		public bool TryRollback()
		{
			var position = new UnityEngine.Vector2Int(_component.X, _component.Y);
			var settings = new ComponentSettings(_component.KeyBinding, _component.Behaviour, _component.Locked);
			return _shipEditor.TryInstallComponent(_component.Location, position, _component.Info, settings);
		}
	}
}
