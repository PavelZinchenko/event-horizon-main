using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameDatabase.Extensions;
using GameDatabase.Enums;
using Constructor.Ships;
using Constructor;

namespace ShipEditor.Model
{
	public interface IComponentTracker
	{
		void OnComponentAdded(Component component);
		void OnComponentRemoved(Component component);
		void OnKeyBindingChanged(Component component, int keyBinding);
	}

	public interface ICompatibilityChecker
	{
		bool IsCompatible(Satellite satellite);
		bool IsCompatible(Component component);
		bool UniqueComponentInstalled(Component component);
		int GetDefaultKey(Component component);
	}

	public class ComponentTracker : ICompatibilityChecker, IComponentTracker
	{
		private const int _actionKeyCount = 6;

		private readonly IShip _ship;
		private readonly HashSet<string> _uniqueids = new();
		private readonly Dictionary<Component, int> _keyBindings = new();

		public ComponentTracker(IShip ship)
		{
			_ship = ship;
		}

		public bool UniqueComponentInstalled(Component component)
		{
			var key = component.GetUniqueKey();
			return !string.IsNullOrEmpty(key) && _uniqueids.Contains(key);
		}

		public bool IsCompatible(Satellite satellite)
		{
			return _ship.IsSuitableSatelliteSize(satellite);
		}

		public bool IsCompatible(Component component)
		{
			if (!Constructor.Component.CompatibilityChecker.IsCompatibleComponent(component, _ship.Model))
				return false;

			if (UniqueComponentInstalled(component)) 
				return false;

			return true;
		}

		public int GetDefaultKey(Component component)
		{
			if (component.GetActivationType() == ActivationType.None)
				return 0;

			if (_keyBindings.TryGetValue(component, out var key))
				return key;

			var usedKeys = _keyBindings.Values.ToHashSet();
			for (int i = 0; i < _actionKeyCount; ++i)
				if (!usedKeys.Contains(i)) return i;

			return 0;
		}

		public void OnComponentAdded(Component component)
		{
			var key = component.GetUniqueKey();
			if (!string.IsNullOrEmpty(key) && !_uniqueids.Add(key))
				throw new InvalidOperationException();
		}

		public void OnComponentRemoved(Component component)
		{
			var key = component.GetUniqueKey();
			if (!string.IsNullOrEmpty(key))
				_uniqueids.Remove(key);

			_keyBindings.Remove(component);
		}

		public void OnKeyBindingChanged(Component component, int keyBinding)
		{
			if (component.GetActivationType() == ActivationType.None) return;
			_keyBindings[component] = keyBinding;
		}
	}
}
