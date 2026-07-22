//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class Component
	{
		partial void OnDataDeserialized(ComponentSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentSerializable serializable);

		public static Component Create(ComponentSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Component(serializable, database);
		}

		private Component(ComponentSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Component>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				Description = serializable.Description;
				DisplayCategory = serializable.DisplayCategory;
				Availability = serializable.Availability;
				Stats = database.GetComponentStatsId(serializable.ComponentStatsId);
				if (Stats.IsNull)
				    throw new DatabaseException(this.GetType().Name + " (" + serializable.Id + "): Stats cannot be null");
				Faction = database.GetFactionId(serializable.Faction);
				Level = new NumericValue<int>(serializable.Level, 0, 2147483647);
				Icon = serializable.Icon;
				Color = Helpers.ColorFromString(serializable.Color);
				Layout = new Layout(serializable.Layout);
				Device = database.GetDeviceId(serializable.DeviceId);
				Weapon = database.GetWeaponId(serializable.WeaponId);
				Ammunition = database.GetAmmunitionId(serializable.AmmunitionId);
				AmmunitionObsolete = database.GetAmmunitionObsoleteId(serializable.AmmunitionId);
				WeaponSlotType = string.IsNullOrEmpty(serializable.WeaponSlotType) ? default : serializable.WeaponSlotType[0];
				DroneBay = database.GetDroneBayId(serializable.DroneBayId);
				Drone = database.GetShipBuildId(serializable.DroneId);
				Restrictions.Value = DataModel.ComponentRestrictions.Create(serializable.Restrictions, database);
				PossibleModifications = serializable.PossibleModifications?.Select(id => new Wrapper<ComponentMod> { Item = database.GetComponentModId(id) }).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ComponentSerializable serializable)
		{
			serializable.Name = Name;
			serializable.Description = Description;
			serializable.DisplayCategory = DisplayCategory;
			serializable.Availability = Availability;
			serializable.ComponentStatsId = Stats.Value;
			serializable.Faction = Faction.Value;
			serializable.Level = Level.Value;
			serializable.Icon = Icon;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.Layout = Layout.Data;
			serializable.DeviceId = Device.Value;
			serializable.WeaponId = Weapon.Value;
			serializable.AmmunitionId = Ammunition.Value;
			serializable.AmmunitionId = AmmunitionObsolete.Value;
			serializable.WeaponSlotType = WeaponSlotType == default ? string.Empty : WeaponSlotType.ToString();
			serializable.DroneBayId = DroneBay.Value;
			serializable.DroneId = Drone.Value;
			serializable.Restrictions = Restrictions.Value?.Serialize();
			if (PossibleModifications == null || PossibleModifications.Length == 0)
			    serializable.PossibleModifications = null;
			else
			    serializable.PossibleModifications = PossibleModifications.Select(wrapper => wrapper.Item.Value).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Component> Id;

		public string Name;
		public string Description;
		public ComponentCategory DisplayCategory;
		public Availability Availability;
		public ItemId<ComponentStats> Stats = ItemId<ComponentStats>.Empty;
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
		public NumericValue<int> Level = new NumericValue<int>(0, 0, 2147483647);
		public string Icon;
		public System.Drawing.Color Color;
		public Layout Layout;
		public ItemId<Device> Device = ItemId<Device>.Empty;
		public ItemId<Weapon> Weapon = ItemId<Weapon>.Empty;
		public ItemId<Ammunition> Ammunition = ItemId<Ammunition>.Empty;
		public ItemId<AmmunitionObsolete> AmmunitionObsolete = ItemId<AmmunitionObsolete>.Empty;
		public char WeaponSlotType;
		public ItemId<DroneBay> DroneBay = ItemId<DroneBay>.Empty;
		public ItemId<ShipBuild> Drone = ItemId<ShipBuild>.Empty;
		public ObjectWrapper<ComponentRestrictions> Restrictions = new(DataModel.ComponentRestrictions.DefaultValue);
		public Wrapper<ComponentMod>[] PossibleModifications;

		public static Component DefaultValue { get; private set; }
	}
}
