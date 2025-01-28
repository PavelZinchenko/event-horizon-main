//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class Component 
	{
		partial void OnDataDeserialized(ComponentSerializable serializable, Database.Loader loader);

		public static Component Create(ComponentSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new Component(serializable, loader);
		}

		private Component(ComponentSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<Component>(serializable.Id);
			loader.AddComponent(serializable.Id, this);

			Name = serializable.Name;
			Description = serializable.Description;
			DisplayCategory = serializable.DisplayCategory;
			Availability = serializable.Availability;
			Stats = loader?.GetComponentStats(new ItemId<ComponentStats>(serializable.ComponentStatsId)) ?? ComponentStats.DefaultValue;
			if (loader != null && Stats == null)
			    throw new DatabaseException("ObjectTemplate.Stats cannot be null - " + serializable.ComponentStatsId);
			Faction = loader?.GetFaction(new ItemId<Faction>(serializable.Faction)) ?? Faction.DefaultValue;
			Level = UnityEngine.Mathf.Clamp(serializable.Level, 0, 2147483647);
			Icon = new SpriteId(serializable.Icon, SpriteId.Type.Component);
			Color = new ColorData(serializable.Color);
			Layout = new Layout(serializable.Layout);
			Device = loader?.GetDevice(new ItemId<Device>(serializable.DeviceId)) ?? Device.DefaultValue;
			Weapon = loader?.GetWeapon(new ItemId<Weapon>(serializable.WeaponId)) ?? Weapon.DefaultValue;
			Ammunition = loader?.GetAmmunition(new ItemId<Ammunition>(serializable.AmmunitionId)) ?? Ammunition.DefaultValue;
			AmmunitionObsolete = loader?.GetAmmunitionObsolete(new ItemId<AmmunitionObsolete>(serializable.AmmunitionId)) ?? AmmunitionObsolete.DefaultValue;
			WeaponSlotType = string.IsNullOrEmpty(serializable.WeaponSlotType) ? default : serializable.WeaponSlotType[0];
			DroneBay = loader?.GetDroneBay(new ItemId<DroneBay>(serializable.DroneBayId)) ?? DroneBay.DefaultValue;
			Drone = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.DroneId)) ?? ShipBuild.DefaultValue;
			Restrictions = ComponentRestrictions.Create(serializable.Restrictions, loader);
			PossibleModifications = new ImmutableCollection<ComponentMod>(serializable.PossibleModifications?.Select(item => loader.GetComponentMod(new ItemId<ComponentMod>(item), true)));

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<Component> Id;

		public string Name { get; private set; }
		public string Description { get; private set; }
		public ComponentCategory DisplayCategory { get; private set; }
		public Availability Availability { get; private set; }
		public ComponentStats Stats { get; private set; }
		public Faction Faction { get; private set; }
		public int Level { get; private set; }
		public SpriteId Icon { get; private set; }
		public ColorData Color { get; private set; }
		public Layout Layout { get; private set; }
		public Device Device { get; private set; }
		public Weapon Weapon { get; private set; }
		public Ammunition Ammunition { get; private set; }
		public AmmunitionObsolete AmmunitionObsolete { get; private set; }
		public char WeaponSlotType { get; private set; }
		public DroneBay DroneBay { get; private set; }
		public ShipBuild Drone { get; private set; }
		public ComponentRestrictions Restrictions { get; private set; }
		public ImmutableCollection<ComponentMod> PossibleModifications { get; private set; }

		public static Component DefaultValue { get; private set; }
	}
}
