using GameDatabase.DataModel;
using Constructor;

namespace ShipEditor.Model
{
	public interface IComponentModel
	{
		int Id { get; }
		int X { get; }
		int Y { get; }
		int KeyBinding { get; }
		int Behaviour { get; }
		bool Locked { get; }
		Component Data { get; }
		ComponentInfo Info { get; }
	}

	public readonly struct ComponentSettings
	{
		public ComponentSettings(int keyBinding, int behaviour, bool locked)
		{
			KeyBinding = keyBinding;
			Behaviour = behaviour;
			Locked = locked;
		}

		public readonly bool Locked;
		public readonly int KeyBinding;
		public readonly int Behaviour;
	}

	public class ComponentModel : IComponentModel
	{
		public ComponentModel(int id, int x, int y, ComponentInfo component, ComponentSettings settings)
		{
			Id = id;
			X = x;
			Y = y;
			Settings = settings;
			Info = component;
		}

		public Component Data => Info.Data;
		public ComponentInfo Info { get; }
		public int Id { get; set; }
		public int X { get; }
		public int Y { get; }
		public bool Locked => Settings.Locked;
		public int KeyBinding => Settings.KeyBinding;
		public int Behaviour => Settings.Behaviour;
		public ComponentSettings Settings { get; set; }
	}
}
