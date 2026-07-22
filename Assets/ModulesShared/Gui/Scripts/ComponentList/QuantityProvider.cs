using Constructor;
using GameDatabase.DataModel;
using Utilites.Collections;

namespace Gui.ComponentList
{
    public interface IComponentQuantityProvider
    {
        int GetQuantity(ComponentInfo component);
    }

    public class ComponentQuantityProvider : IComponentQuantityProvider
    {
        public ComponentQuantityProvider(IReadOnlyInventory<ComponentInfo> components)
        {
            _components = components;
        }

        public int GetQuantity(ComponentInfo component)
        {
            return _components.GetQuantity(component);
        }

        private readonly IReadOnlyInventory<ComponentInfo> _components;
    }

    public class BlueprintQuantityProvider : IComponentQuantityProvider
    {
        public BlueprintQuantityProvider(IReadOnlyInventory<Component> blueprints)
        {
            _blueprints = blueprints;
        }

        public int GetQuantity(ComponentInfo component)
        {
            return _blueprints.GetQuantity(component.Data);
        }

        private readonly IReadOnlyInventory<Component> _blueprints;
    }
}
