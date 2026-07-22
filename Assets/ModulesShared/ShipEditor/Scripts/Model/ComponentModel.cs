using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Model;

namespace ShipEditor.Model
{
	public interface IComponentModel
	{
		int Id { get; }
		int X { get; }
		int Y { get; }
		int KeyBinding { get; }
		int Behaviour { get; }
		int PersistedBarrelId { get; }
		int Rotation { get; }
		bool Locked { get; }
		Component Data { get; }
		ComponentInfo Info { get; }
		Layout Layout { get; }
		ShipElementType Location { get; }
	}

	public readonly struct ComponentSettings
	{
		public ComponentSettings(int keyBinding, int behaviour, bool locked, int persistedBarrelId = int.MinValue, int rotation = 0)
		{
			KeyBinding = keyBinding;
			Behaviour = behaviour;
			Locked = locked;
			PersistedBarrelId = persistedBarrelId;
			Rotation = rotation & 3;
		}

		public readonly bool Locked;
		public readonly int KeyBinding;
		public readonly int Behaviour;
		public readonly int PersistedBarrelId;
		public readonly int Rotation;
	}

	public class ComponentModel : IComponentModel
	{
		public ComponentModel(int id, int x, int y, ComponentInfo component, ComponentSettings settings, ShipElementType location)
		{
			Id = id;
			X = x;
			Y = y;
			Settings = settings;
			Info = component;
			Location = location;
		}

		public Component Data => Info.Data;
		public ComponentInfo Info { get; private set; }
		public int Id { get; set; }
		public int X { get; }
		public int Y { get; }
		public bool Locked => Settings.Locked;
		public int KeyBinding => Settings.KeyBinding;
		public int Behaviour => Settings.Behaviour;
		public int PersistedBarrelId => Settings.PersistedBarrelId;
		public int Rotation => Settings.Rotation;
		public Layout Layout
		{
			get
			{
				if (_layoutRotation != Rotation)
				{
					_layout = ComponentLayoutRotation.Get(Data.Layout, Rotation);
					_layoutRotation = Rotation;
				}

				return _layout;
			}
		}

		public ComponentSettings Settings { get; set; }
		public ShipElementType Location { get; }

		public void SetInfo(ComponentInfo info)
		{
			Info = info;
		}

		private Layout _layout;
		private int _layoutRotation = -1;
	}
}
