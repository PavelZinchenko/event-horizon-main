using System;
using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
using GameDatabase.Model;
using Utils;

namespace Gui.ComponentList
{
	public class SatellitesNode : IComponentTreeNode
	{
		public enum Location
		{
			Left,
			Right,
		}

		public SatellitesNode(IComponentTreeNode parent, Location location)
		{
			Parent = parent;

			switch (location)
			{
				case Location.Left: Name = "$GroupLeftSatellite"; break;
				case Location.Right: Name = "$GroupRightSatellite"; break;
			}
		}

		public IComponentTreeNode Parent { get; }

		public IComponentQuantityProvider QuantityProvider => null;

		public bool IsVisible { get; set; } = true;

		public string Name { get; }
        public SpriteId Icon => new SpriteId("textures/icons/icon_scanner", SpriteId.Type.Default);
        public UnityEngine.Color Color => CommonNode.DefaultColor;
        public void Add(ComponentInfo componentInfo) { throw new InvalidOperationException(); }
        public int ItemCount => 10; // TODO;
        public IEnumerable<IComponentTreeNode> Nodes => Enumerable.Empty<IComponentTreeNode>();
        public IEnumerable<ComponentInfo> Components => Enumerable.Empty<ComponentInfo>();

        public void Clear() { }
	}

	//public class ShipQuantityProvider : IComponentQuantityProvider
	//{
	//    public ShipQuantityProvider(IShip ship)
	//    {
	//        _ship = ship;
	//        Reset();
	//    }

	//    public int GetQuantity(ComponentInfo component)
	//    {
	//        return Components.GetQuantity(component);
	//    }

	//    public void Reset()
	//    {
	//        _components.Clear();
	//        _notInitialized = true;
	//    }

	//    public IGameItemCollection<ComponentInfo> Components
	//    {
	//        get
	//        {
	//            if (_notInitialized)
	//            {
	//                foreach (var item in _ship.Components)
	//                    _components.Add(item.Info);

	//                _notInitialized = false;
	//            }

	//            return _components;
	//        }
	//    }

	//    private bool _notInitialized;
	//    private readonly IShip _ship;
	//    private readonly IGameItemCollection<ComponentInfo> _components = new GameItemCollection<ComponentInfo>();
	//}
}
