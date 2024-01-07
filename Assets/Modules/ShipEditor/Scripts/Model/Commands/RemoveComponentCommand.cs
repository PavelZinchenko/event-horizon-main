namespace ShipEditor.Model
{
	public class RemoveComponentCommand : ICommand
	{
		private readonly IShipEditorModel _shipEditor;
		private readonly IComponentModel _model;
		private ShipElementType _shipElement;

		public bool BelongsToElement(ShipElementType shipElement) => shipElement == _shipElement;

		public RemoveComponentCommand(IShipEditorModel shipEditor, IComponentModel model)
		{
			_shipEditor = shipEditor;
			_model = model;
		}

		public bool TryExecute()
		{
			if (!_shipEditor.TryFindComponent(_model, out _shipElement)) return false;
			_shipEditor.RemoveComponent(_shipElement, _model);
			return true;
		}

		public bool TryRollback()
		{
			var position = new UnityEngine.Vector2Int(_model.X, _model.Y);
			var settings = new ComponentSettings(_model.KeyBinding, _model.Behaviour, _model.Locked);
			return _shipEditor.TryInstallComponent(position, _shipElement, _model.Info, settings);
		}
	}
}
