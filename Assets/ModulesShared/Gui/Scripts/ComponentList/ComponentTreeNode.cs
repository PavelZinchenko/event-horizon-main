using System.Collections.Generic;
using System.Linq;
using Constructor;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Gui.ComponentList
{
    public class RootNode : IComponentTreeNode
    {
        public RootNode(IComponentQuantityProvider quantityProvider, IDatabase database = null)
        {
            if (database != null)
                _weaponNode = new WeaponNode(this, database.WeaponSlots);

            _quantityProvider = quantityProvider;
            _armorNode = CreateNode("$GroupArmor", new SpriteId("icons/icon_shield", SpriteId.Type.Default));
            _energyNode = CreateNode("$GroupEnergy", new SpriteId("icons/icon_battery", SpriteId.Type.Default));
            _droneNode = CreateNode("$GroupDrones", new SpriteId("icons/icon_drone", SpriteId.Type.Default));
            _engineNode = CreateNode("$GroupEngines", new SpriteId("icons/icon_engine", SpriteId.Type.Default));
            _specialNode = CreateNode("$GroupSpecial", new SpriteId("icons/icon_gear", SpriteId.Type.Default));
            if (database != null)
            {
                _originalBranch = new FactionCategoryNode("原版", this, database.WeaponSlots);
                _modBranch = new FactionCategoryNode("三体模组", this, database.WeaponSlots);
                _otherModBranch = new FactionCategoryNode("其他模组", this, database.WeaponSlots);
            }

            IsVisible = true;
        }

        public void AddNode(IComponentTreeNode node, bool inTheEnd = false)
        {
            if (inTheEnd)
                _extraNodes2.Add(node);
            else
                _extraNodes1.Add(node);
        }

        public IComponentTreeNode Parent { get { return null; } }
        public IComponentQuantityProvider QuantityProvider { get { return _quantityProvider; } }

        public IComponentTreeNode Weapon { get { return _weaponNode; } }
        public IComponentTreeNode Armor { get { return _armorNode; } }
        public IComponentTreeNode Drone { get { return _droneNode; } }
        public IComponentTreeNode Engine { get { return _engineNode; } }
        public IComponentTreeNode Energy { get { return _energyNode; } }
        public IComponentTreeNode Special { get { return _specialNode; } }

        public string Name { get { return "$GroupAll"; } }
        public SpriteId Icon { get { return new SpriteId("icons/icon_gear", SpriteId.Type.Default); } }
        public UnityEngine.Color Color { get { return CommonNode.DefaultColor; } }
        public bool IsVisible { get; set; }

        public void Add(ComponentInfo componentInfo)
        {
            if (_originalBranch != null)
            {
                var component = componentInfo.Data;
                if (component.ContentSource == ContentSource.ThreeBody)
                    _modBranch.Add(componentInfo);
                else if (component.Id.Value <= LastOriginalComponentId)
                    _originalBranch.Add(componentInfo);
                else
                    _otherModBranch.Add(componentInfo);
                _count = -1;
                return;
            }

            if (componentInfo.Data.Weapon != null)
            {
                _weaponNode.Add(componentInfo);
                return;
            }

            switch (componentInfo.Data.DisplayCategory)
            {
                case ComponentCategory.Defense:
                    _armorNode.Add(componentInfo);
                    break;
                case ComponentCategory.Energy:
                    _energyNode.Add(componentInfo);
                    break;
                case ComponentCategory.Engine:
                    _engineNode.Add(componentInfo);
                    break;
                case ComponentCategory.Drones:
                    _droneNode.Add(componentInfo);
                    break;
                default:
                    _specialNode.Add(componentInfo);
                    break;
            }

            _count = -1;
        }

        public int ItemCount
        {
            get
            {
                if (_count < 0)
                    _count = Children.GetItemCount();

                return _count;
            }
        }

        public IEnumerable<IComponentTreeNode> Nodes
        {
            get
            {
                return Children.ChildrenNodes();
            }
        }

        public IEnumerable<ComponentInfo> Components { get { return Children.ChildrenComponents(); } }

        public void Clear() { Children.Clear(); }

        private IEnumerable<IComponentTreeNode> Children
        {
            get
            {
                foreach (var node in _extraNodes1)
                    yield return node;

                if (_originalBranch != null)
                {
                    yield return _originalBranch;
                    yield return _modBranch;
                    yield return _otherModBranch;
                    foreach (var node in _extraNodes2)
                        yield return node;
                    yield break;
                }

                yield return _weaponNode;
                yield return _armorNode;
                yield return _energyNode;
                yield return _droneNode;
                yield return _engineNode;
                yield return _specialNode;

                foreach (var node in _extraNodes2)
                    yield return node;

            }
        }

        private IComponentTreeNode CreateNode(string name, SpriteId icon)
        {
            return new CommonNode(name, icon, this);
        }

        private int _count = -1;
        private readonly IComponentTreeNode _weaponNode;
        private readonly IComponentTreeNode _armorNode;
        private readonly IComponentTreeNode _energyNode;
        private readonly IComponentTreeNode _droneNode;
        private readonly IComponentTreeNode _engineNode;
        private readonly IComponentTreeNode _specialNode;
        private readonly IComponentQuantityProvider _quantityProvider;
        private readonly List<IComponentTreeNode> _extraNodes1 = new List<IComponentTreeNode>();
        private readonly List<IComponentTreeNode> _extraNodes2 = new List<IComponentTreeNode>();
        private readonly FactionCategoryNode _originalBranch;
        private readonly FactionCategoryNode _modBranch;
        private readonly FactionCategoryNode _otherModBranch;
        private const int LastOriginalComponentId = 299;
    }

    public sealed class FactionCategoryNode : IComponentTreeNode
    {
        public FactionCategoryNode(string name, IComponentTreeNode parent, WeaponSlots weaponSlots)
        {
            Name = name;
            Parent = parent;
            _weapon = new WeaponNode(this, weaponSlots);
            _armor = New("$GroupArmor", "icons/icon_shield");
            _energy = New("$GroupEnergy", "icons/icon_battery");
            _drone = New("$GroupDrones", "icons/icon_drone");
            _engine = New("$GroupEngines", "icons/icon_engine");
            _special = New("$GroupSpecial", "icons/icon_gear");
        }

        public IComponentTreeNode Parent { get; }
        public IComponentQuantityProvider QuantityProvider => Parent.QuantityProvider;
        public string Name { get; }
        public SpriteId Icon => new SpriteId("icons/icon_gear", SpriteId.Type.Default);
        public UnityEngine.Color Color => CommonNode.DefaultColor;
        public bool IsVisible => true;
        public IEnumerable<IComponentTreeNode> Nodes => Children.ChildrenNodes();
        public IEnumerable<ComponentInfo> Components => Children.ChildrenComponents();
        public int ItemCount => Children.GetItemCount();

        public void Add(ComponentInfo item)
        {
            if (item.Data.Weapon != null) { _weapon.Add(item); return; }
            switch (item.Data.DisplayCategory)
            {
                case ComponentCategory.Defense: _armor.Add(item); break;
                case ComponentCategory.Energy: _energy.Add(item); break;
                case ComponentCategory.Engine: _engine.Add(item); break;
                case ComponentCategory.Drones: _drone.Add(item); break;
                default: _special.Add(item); break;
            }
        }

        public void Clear() => Children.Clear();

        private CommonNode New(string name, string icon) => new CommonNode(name, new SpriteId(icon, SpriteId.Type.Default), this);
        private IEnumerable<IComponentTreeNode> Children
        {
            get
            {
                yield return _weapon;
                yield return _armor;
                yield return _energy;
                yield return _drone;
                yield return _engine;
                yield return _special;
            }
        }

        private readonly WeaponNode _weapon;
        private readonly CommonNode _armor;
        private readonly CommonNode _energy;
        private readonly CommonNode _drone;
        private readonly CommonNode _engine;
        private readonly CommonNode _special;
    }

    public class WeaponNode : IComponentTreeNode
    {
        public WeaponNode(IComponentTreeNode parent, WeaponSlots weaponSlots)
        {
            _parent = parent;

            // WeaponSlots is user/mod content and older saves or third-party mods may
            // provide a settings file that predates one of the built-in groups.  The
            // component list must still be able to classify every component in that
            // case; otherwise D-slot components (and any future slot) are reported as
            // "Undefined weapon slot" and presets appear to fail to load.
            foreach (var slot in weaponSlots?.Slots ?? Enumerable.Empty<WeaponSlot>())
                if (_groupMap.TryAdd(slot.Letter, _groups.Count))
                    _groups.Add(CreateNode(slot.Name, slot.Icon));
                else
                    GameDiagnostics.Trace.LogError($"Duplicate weapon slot - {slot.Letter}");

            EnsureBuiltinSlot('C', "$GroupWeaponC", "icon_weapon_c");
            EnsureBuiltinSlot('L', "$GroupWeaponL", "icon_weapon_l");
            EnsureBuiltinSlot('M', "$GroupWeaponM", "icon_weapon_m");
            EnsureBuiltinSlot('T', "$GroupWeaponT", "icon_weapon_t");
            EnsureBuiltinSlot('S', "$GroupWeaponS", "icon_weapon_s");
            EnsureBuiltinSlot('D', "$GroupWeaponD", "icon_weapon_s");

            // A malformed/legacy database can contain a default slot entry with
            // the NUL letter.  Do not throw when it is already present.
            if (!_groupMap.ContainsKey(default))
                _groupMap.Add(default, _groups.Count);
            _groups.Add(CreateNode(weaponSlots?.DefaultSlotName ?? "$GroupWeaponAny",
                weaponSlots?.DefaultSlotIcon ?? new SpriteId("icon_weapon_s", SpriteId.Type.GuiIcon)));
        }

        private void EnsureBuiltinSlot(char letter, string name, string icon)
        {
            if (_groupMap.ContainsKey(letter)) return;

            _groupMap.Add(letter, _groups.Count);
            _groups.Add(CreateNode(name, new SpriteId(icon, SpriteId.Type.GuiIcon)));
        }

        public IComponentTreeNode Parent { get { return _parent; } }
        public IComponentQuantityProvider QuantityProvider { get { return _parent.QuantityProvider; } }

        public string Name { get { return "$GroupWeapon"; } }
        public SpriteId Icon { get { return new SpriteId("textures/icons/icon_weapon", SpriteId.Type.Default); } }
        public UnityEngine.Color Color { get { return CommonNode.DefaultColor; } }

        public void Add(ComponentInfo componentInfo)
        {
            var weapon = componentInfo.Data.Weapon;
            if (weapon == null)
            {
                GameDiagnostics.Trace.LogError("WeaponNode: component is not weapon - " + componentInfo.Data.Id);
                return;
            }

            var letter = (char)componentInfo.Data.WeaponSlotType;
            if (!_groupMap.TryGetValue(letter, out var groupId))
            {
                // Keep old saves and third-party databases loadable when they
                // contain a newly introduced slot (notably D) but their slot
                // table was serialized before that slot existed.  Add a visible
                // group on demand instead of reporting an error and silently
                // putting the component in the Any group.
                EnsureBuiltinSlot(letter, "$GroupWeapon" + letter, "icon_weapon_s");
                if (!_groupMap.TryGetValue(letter, out groupId))
                    groupId = _groups.Count - 1;
            }

            _groups[groupId].Add(componentInfo);
        }

        public int ItemCount
        {
            get
            {
                if (_count < 0)
                    _count = Children.GetItemCount();

                return _count;
            }
        }

        public IEnumerable<IComponentTreeNode> Nodes { get { return Children.ChildrenNodes(); } }
        public IEnumerable<ComponentInfo> Components { get { return Children.ChildrenComponents(); } }
        public void Clear() { Children.Clear(); }
        public bool IsVisible => true;

        private IEnumerable<IComponentTreeNode> Children => _groups;

        private IComponentTreeNode CreateNode(string name, SpriteId icon)
        {
            return new CommonNode(name, icon, this);
        }

        private int _count = -1;
        private readonly IComponentTreeNode _parent;
        private readonly Dictionary<char,int> _groupMap = new();
        private readonly List<IComponentTreeNode> _groups = new();
    }

    public class ComponentNode : IComponentTreeNode
    {
        public ComponentNode(Component component, IComponentTreeNode parent)
        {
            _component = component;
            _parent = parent;
        }

        public IComponentTreeNode Parent { get { return _parent; } }
        public IComponentQuantityProvider QuantityProvider { get { return _parent.QuantityProvider; } }

        public string Name { get { return _component.Name; } }
        public SpriteId Icon { get { return _component.Icon; } }
        public UnityEngine.Color Color { get { return _component.Color; } }
        public bool IsVisible => true;

        public void Add(ComponentInfo componentInfo)
        {
            if (componentInfo.Data.Id != _component.Id)
            {
                GameDiagnostics.Trace.LogError("ComponentNode: wrong component id - " + componentInfo.Data.Id);
                return;
            }

            _components.Add(componentInfo);
        }

        public int ItemCount { get { return _components.Count; } }
        public IEnumerable<IComponentTreeNode> Nodes { get { return Enumerable.Empty<IComponentTreeNode>(); } }
        public IEnumerable<ComponentInfo> Components { get { return _components; } }

        public void Clear()
        {
            _components.Clear();
        }

        private readonly Component _component;
        private readonly IComponentTreeNode _parent;
        private readonly HashSet<ComponentInfo> _components = new HashSet<ComponentInfo>();
    }

    public class CommonNode : IComponentTreeNode
    {
        public CommonNode(string name, SpriteId icon, IComponentTreeNode parent)
        {
            _parent = parent;
            _name = name;
            _icon = icon;
        }

        public IComponentTreeNode Parent { get { return _parent; } }
        public IComponentQuantityProvider QuantityProvider { get { return _parent.QuantityProvider; } }

        public string Name { get { return _name; } }
        public SpriteId Icon { get { return _icon; } }
        public UnityEngine.Color Color { get { return DefaultColor; } }
        public bool IsVisible => true;

        public void Add(ComponentInfo componentInfo)
        {
            var component = componentInfo.Data;
            IComponentTreeNode node;
            if (!_components.TryGetValue(component.Id.Value, out node))
            {
                node = new ComponentNode(component, this);
                _components.Add(component.Id.Value, node);
            }

            node.Add(componentInfo);
            _count = -1;
        }

        public int ItemCount
        {
            get
            {
                if (_count < 0)
                    _count = _components.Values.GetItemCount();

                return _count;
            }
        }

        public IEnumerable<IComponentTreeNode> Nodes { get { return _components.Values.Where(ComponentTreeNodeExtensions.ShouldNotExpand); } }
        public IEnumerable<ComponentInfo> Components { get { return _components.Values.ChildrenComponents(); } }

        public void Clear()
        {
            _components.Values.Clear();
        }

        private int _count;
        private readonly string _name;
        private readonly SpriteId _icon;
        private readonly IComponentTreeNode _parent;
        private readonly Dictionary<int, IComponentTreeNode> _components = new Dictionary<int, IComponentTreeNode>();

        public static readonly UnityEngine.Color DefaultColor = Gui.Theme.UiTheme.Current.GetColor(Theme.ThemeColor.ButtonIcon);
    }

    public class ComponentListNode : IComponentTreeNode
    {
        public ComponentListNode(string name, SpriteId icon, IComponentTreeNode parent)
        {
            _parent = parent;
            _name = name;
            _icon = icon;
        }

        public IComponentTreeNode Parent { get { return _parent; } }
        public IComponentQuantityProvider QuantityProvider { get { return _parent.QuantityProvider; } }

        public string Name { get { return _name; } }
        public SpriteId Icon { get { return _icon; } }
        public UnityEngine.Color Color { get { return CommonNode.DefaultColor; } }
        public bool IsVisible => true;

        public void Add(ComponentInfo componentInfo)
        {
            _components.Add(componentInfo);
        }

        public int ItemCount { get { return _components.Count; } }

        public IEnumerable<IComponentTreeNode> Nodes { get { return Enumerable.Empty<IComponentTreeNode>(); } }
        public IEnumerable<ComponentInfo> Components { get { return _components; } }

        public void Clear()
        {
            _components.Clear();
        }

        private readonly string _name;
        private readonly SpriteId _icon;
        private readonly IComponentTreeNode _parent;
        private readonly HashSet<ComponentInfo> _components = new HashSet<ComponentInfo>();
    }
}
