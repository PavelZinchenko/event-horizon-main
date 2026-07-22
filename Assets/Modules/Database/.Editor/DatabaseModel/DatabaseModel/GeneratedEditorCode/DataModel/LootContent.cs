//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{

	public interface ILootContentContent
	{
		void Load(LootContentSerializable serializable, Database database);
		void Save(ref LootContentSerializable serializable);
	}

	public partial class LootContent : IDataAdapter
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		private static ILootContentContent CreateContent(LootItemType type)
		{
			switch (type)
			{
				case LootItemType.None:
					return new LootContentEmptyContent();
				case LootItemType.SomeMoney:
					return new LootContent_SomeMoney();
				case LootItemType.Fuel:
					return new LootContent_Fuel();
				case LootItemType.Money:
					return new LootContent_Money();
				case LootItemType.Stars:
					return new LootContent_Stars();
				case LootItemType.StarMap:
					return new LootContentEmptyContent();
				case LootItemType.RandomComponents:
					return new LootContent_RandomComponents();
				case LootItemType.RandomItems:
					return new LootContent_RandomItems();
				case LootItemType.AllItems:
					return new LootContent_AllItems();
				case LootItemType.ItemsWithChance:
					return new LootContent_ItemsWithChance();
				case LootItemType.QuestItem:
					return new LootContent_QuestItem();
				case LootItemType.Ship:
					return new LootContent_Ship();
				case LootItemType.EmptyShip:
					return new LootContent_EmptyShip();
				case LootItemType.Component:
					return new LootContent_Component();
				case LootItemType.Blueprint:
					return new LootContent_Blueprint();
				case LootItemType.ResearchPoints:
					return new LootContent_ResearchPoints();
				case LootItemType.Satellite:
					return new LootContent_Satellite();
				default:
					throw new DatabaseException("LootContent: Invalid content type - " + type);
			}
		}

		public static LootContent Create(LootContentSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new LootContent(serializable, database);
		}

		public LootContent()
		{
			_content = new LootContentEmptyContent();
		}

		private LootContent(LootContentSerializable serializable, Database database)
		{
			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public LootContentSerializable Serialize()
		{
			var serializable = new LootContentSerializable();
			serializable.ItemId = 0;
			serializable.MinAmount = 0;
			serializable.MaxAmount = 0;
			serializable.ValueRatio = 0f;
			serializable.Factions = null;
			serializable.Items = null;
			_content.Save(ref serializable);
			serializable.Type = Type;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public event System.Action LayoutChangedEvent;
		public event System.Action DataChangedEvent;

		public IEnumerable<IProperty> Properties
		{
			get
			{
				var type = GetType();

				yield return new Property(this, type.GetField("Type"), OnTypeChanged);

				foreach (var item in _content.GetType().GetFields().Where(f => f.IsPublic && !f.IsStatic))
					yield return new Property(_content, item, DataChangedEvent);
			}
		}

		private void OnTypeChanged()
		{
			_content = CreateContent(Type);
			DataChangedEvent?.Invoke();
			LayoutChangedEvent?.Invoke();
		}

		private ILootContentContent _content;
		public LootItemType Type;

		public static LootContent DefaultValue { get; private set; }
	}

	public class LootContentEmptyContent : ILootContentContent
	{
		public void Load(LootContentSerializable serializable, Database database) {}
		public void Save(ref LootContentSerializable serializable) {}
	}

	public partial class LootContent_SomeMoney : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			ValueRatio = new NumericValue<float>(serializable.ValueRatio, 0.001f, 1000f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ValueRatio = ValueRatio.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> ValueRatio = new NumericValue<float>(0, 0.001f, 1000f);
	}

	public partial class LootContent_Fuel : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

	public partial class LootContent_Money : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

	public partial class LootContent_Stars : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

	public partial class LootContent_RandomComponents : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);
			ValueRatio = new NumericValue<float>(serializable.ValueRatio, 0.001f, 1000f);
			Factions.Value = DataModel.RequiredFactions.Create(serializable.Factions, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			serializable.ValueRatio = ValueRatio.Value;
			serializable.Factions = Factions.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<float> ValueRatio = new NumericValue<float>(0, 0.001f, 1000f);
		public ObjectWrapper<RequiredFactions> Factions = new(DataModel.RequiredFactions.DefaultValue);
	}

	public partial class LootContent_RandomItems : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);
			Items = serializable.Items?.Select(item => LootItem.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			if (Items == null || Items.Length == 0)
			    serializable.Items = null;
			else
			    serializable.Items = Items.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
		public LootItem[] Items;
	}

	public partial class LootContent_AllItems : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Items = serializable.Items?.Select(item => LootItem.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			if (Items == null || Items.Length == 0)
			    serializable.Items = null;
			else
			    serializable.Items = Items.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public LootItem[] Items;
	}

	public partial class LootContent_ItemsWithChance : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Items = serializable.Items?.Select(item => LootItem.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			if (Items == null || Items.Length == 0)
			    serializable.Items = null;
			else
			    serializable.Items = Items.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public LootItem[] Items;
	}

	public partial class LootContent_QuestItem : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			QuestItem = database.GetQuestItemId(serializable.ItemId);
			if (QuestItem.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": QuestItem cannot be null");
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = QuestItem.Value;
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<QuestItem> QuestItem = ItemId<QuestItem>.Empty;
		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

	public partial class LootContent_Ship : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			ShipBuild = database.GetShipBuildId(serializable.ItemId);
			if (ShipBuild.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": ShipBuild cannot be null");

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = ShipBuild.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<ShipBuild> ShipBuild = ItemId<ShipBuild>.Empty;
	}

	public partial class LootContent_EmptyShip : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Ship = database.GetShipId(serializable.ItemId);
			if (Ship.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": Ship cannot be null");

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = Ship.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<Ship> Ship = ItemId<Ship>.Empty;
	}

	public partial class LootContent_Component : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Component = database.GetComponentId(serializable.ItemId);
			if (Component.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": Component cannot be null");
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = Component.Value;
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<Component> Component = ItemId<Component>.Empty;
		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

	public partial class LootContent_Blueprint : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Blueprint = database.GetTechnologyId(serializable.ItemId);
			if (Blueprint.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": Blueprint cannot be null");

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = Blueprint.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<Technology> Blueprint = ItemId<Technology>.Empty;
	}

	public partial class LootContent_ResearchPoints : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);
			Factions.Value = DataModel.RequiredFactions.Create(serializable.Factions, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			serializable.Factions = Factions.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
		public ObjectWrapper<RequiredFactions> Factions = new(DataModel.RequiredFactions.DefaultValue);
	}

	public partial class LootContent_Satellite : ILootContentContent
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootContentSerializable serializable);

		public void Load(LootContentSerializable serializable, Database database)
		{
			Satellite = database.GetSatelliteId(serializable.ItemId);
			if (Satellite.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": Satellite cannot be null");
			MinAmount = new NumericValue<int>(serializable.MinAmount, 0, 999999999);
			MaxAmount = new NumericValue<int>(serializable.MaxAmount, 0, 999999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref LootContentSerializable serializable)
		{
			serializable.ItemId = Satellite.Value;
			serializable.MinAmount = MinAmount.Value;
			serializable.MaxAmount = MaxAmount.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<Satellite> Satellite = ItemId<Satellite>.Empty;
		public NumericValue<int> MinAmount = new NumericValue<int>(0, 0, 999999999);
		public NumericValue<int> MaxAmount = new NumericValue<int>(0, 0, 999999999);
	}

}

