using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameDatabase.Extensions;
using GameDatabase.Enums;
using Constructor.Ships;
using Constructor.Extensions;
using UnityEngine.Rendering;

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
		bool ComponentLimitReached(Component component);
		int GetDefaultKey(Component component);
	}

	public class ComponentTracker : ICompatibilityChecker, IComponentTracker
	{
		private const int _actionKeyCount = 6;

		private readonly IShip _ship;
        private readonly Inventory<Component> _components = new();
		private readonly Inventory<ComponentGroupTag> _tags = new();
		private readonly Dictionary<Component, int> _keyBindings = new();

		public ComponentTracker(IShip ship)
		{
			_ship = ship;
		}

		public bool ComponentLimitReached(Component component)
		{
            var maxAmount = component.Restrictions.MaxComponentAmount;
            if (maxAmount > 0 && _components.Quantity(component) >= maxAmount) return true;

            var tag = component.Restrictions.ComponentGroupTag;
            if (tag != null && _tags.Quantity(tag) >= tag.MaxInstallableComponents) return true;

            return false;
        }

		public bool IsCompatible(Satellite satellite)
		{
			return _ship.IsSuitableSatelliteSize(satellite);
		}

		public bool IsCompatible(Component component)
		{
			if (!Constructor.Component.CompatibilityChecker.IsCompatibleComponent(component, _ship.Model))
				return false;

			if (ComponentLimitReached(component)) 
				return false;

			return true;
		}

		public int GetDefaultKey(Component component)
		{
			if (component.Id.Value == 305)
				return 2;

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
            var maxAmount = component.Restrictions.MaxComponentAmount;
            if (maxAmount > 0)
            {
                var quantity = _components.Add(component);
                if (quantity > maxAmount)
                    GameDiagnostics.Trace.LogError($"Too many {component.Name} were installed: ({quantity}/{maxAmount})");
            }

            var tag = component.Restrictions.ComponentGroupTag;
            if (tag != null)
            {
                var quantity = _tags.Add(tag);
                if (quantity > tag.MaxInstallableComponents)
                    GameDiagnostics.Trace.LogError($"Too many components with the tag '{tag.Id}' were installed: ({quantity}/{tag.MaxInstallableComponents})");
            }
        }

		public void OnComponentRemoved(Component component)
		{
            var maxAmount = component.Restrictions.MaxComponentAmount;
            var tag = component.Restrictions.ComponentGroupTag;

            if (tag != null)
                _tags.Remove(tag);
            if (maxAmount > 0)
                _components.Remove(component);

            _keyBindings.Remove(component);
		}

		public void OnKeyBindingChanged(Component component, int keyBinding)
		{
			if (component.GetActivationType() == ActivationType.None) return;
			_keyBindings[component] = keyBinding;
		}

        private class Inventory<T>
        {
            private readonly Dictionary<T, int> _items = new();

            public int Quantity(T key) => _items.TryGetValue(key, out var quantity) ? quantity : 0;

            public int Add(T key)
            {
                var quantity = Quantity(key) + 1;
                _items[key] = quantity;
                return quantity;
            }

            public bool Remove(T key)
            {
                var quantity = Quantity(key);
                if (quantity == 0) 
                    return false;

                if (quantity == 1)
                {
                    _items.Remove(key);
                    return true;
                }

                _items[key] = quantity - 1;
                return true;
            }
        }
    }
}
