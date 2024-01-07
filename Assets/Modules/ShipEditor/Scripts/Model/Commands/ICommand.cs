using System.Collections.Generic;

namespace ShipEditor.Model
{
	public interface ICommand
	{
		bool TryExecute();
		bool TryRollback();

		bool BelongsToElement(ShipElementType shipElement);
	}

	public class CommandList
	{
		private readonly List<ICommand> _commands = new();

		public event System.Action DataChanged;

		public bool IsEmpty => _commands.Count == 0;

		public void Clear()
		{
			_commands.Clear();
			DataChanged.Invoke();
		}

		public void Clear(ShipElementType shipElementType)
		{
			_commands.RemoveAll(item => item.BelongsToElement(shipElementType));
			DataChanged.Invoke();
		}

		public bool TryExecute(ICommand command)
		{
			if (!command.TryExecute()) return false;

			_commands.Add(command);
			DataChanged.Invoke();
			return true;
		}

		public void Undo()
		{
			if (_commands.Count == 0) return;

			var lastIndex = _commands.Count - 1;
			var command = _commands[lastIndex];
			_commands.RemoveAt(lastIndex);
			DataChanged.Invoke();
			command.TryRollback();
		}
	}
}
