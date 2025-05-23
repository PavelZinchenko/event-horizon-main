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
	public abstract partial class LootContent 
	{
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

		public static LootContent Create(LootContentSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case LootItemType.None:
					return new LootContent_None(serializable, loader);
				case LootItemType.SomeMoney:
					return new LootContent_SomeMoney(serializable, loader);
				case LootItemType.Fuel:
					return new LootContent_Fuel(serializable, loader);
				case LootItemType.Money:
					return new LootContent_Money(serializable, loader);
				case LootItemType.Stars:
					return new LootContent_Stars(serializable, loader);
				case LootItemType.StarMap:
					return new LootContent_StarMap(serializable, loader);
				case LootItemType.RandomComponents:
					return new LootContent_RandomComponents(serializable, loader);
				case LootItemType.RandomItems:
					return new LootContent_RandomItems(serializable, loader);
				case LootItemType.AllItems:
					return new LootContent_AllItems(serializable, loader);
				case LootItemType.ItemsWithChance:
					return new LootContent_ItemsWithChance(serializable, loader);
				case LootItemType.QuestItem:
					return new LootContent_QuestItem(serializable, loader);
				case LootItemType.Ship:
					return new LootContent_Ship(serializable, loader);
				case LootItemType.EmptyShip:
					return new LootContent_EmptyShip(serializable, loader);
				case LootItemType.Component:
					return new LootContent_Component(serializable, loader);
				case LootItemType.Blueprint:
					return new LootContent_Blueprint(serializable, loader);
				case LootItemType.ResearchPoints:
					return new LootContent_ResearchPoints(serializable, loader);
				case LootItemType.Satellite:
					return new LootContent_Satellite(serializable, loader);
				default:
                    throw new DatabaseException("LootContent: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(ILootContentFactory<T> factory);

		protected LootContent(LootContentSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;

			OnDataDeserialized(serializable, loader);
		}


		public LootItemType Type { get; private set; }

		public static LootContent DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface ILootContentFactory<T>
    {
	    T Create(LootContent_None content);
	    T Create(LootContent_SomeMoney content);
	    T Create(LootContent_Fuel content);
	    T Create(LootContent_Money content);
	    T Create(LootContent_Stars content);
	    T Create(LootContent_StarMap content);
	    T Create(LootContent_RandomComponents content);
	    T Create(LootContent_RandomItems content);
	    T Create(LootContent_AllItems content);
	    T Create(LootContent_ItemsWithChance content);
	    T Create(LootContent_QuestItem content);
	    T Create(LootContent_Ship content);
	    T Create(LootContent_EmptyShip content);
	    T Create(LootContent_Component content);
	    T Create(LootContent_Blueprint content);
	    T Create(LootContent_ResearchPoints content);
	    T Create(LootContent_Satellite content);
    }

    public partial class LootContent_None : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_None(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }



    }
    public partial class LootContent_SomeMoney : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_SomeMoney(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			ValueRatio = UnityEngine.Mathf.Clamp(serializable.ValueRatio, 0.001f, 1000f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float ValueRatio { get; private set; }


    }
    public partial class LootContent_Fuel : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Fuel(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }
    public partial class LootContent_Money : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Money(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }
    public partial class LootContent_Stars : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Stars(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }
    public partial class LootContent_StarMap : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_StarMap(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }



    }
    public partial class LootContent_RandomComponents : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_RandomComponents(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);
			ValueRatio = UnityEngine.Mathf.Clamp(serializable.ValueRatio, 0.001f, 1000f);
			Factions = RequiredFactions.Create(serializable.Factions, loader);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }
		public float ValueRatio { get; private set; }
		public RequiredFactions Factions { get; private set; }


    }
    public partial class LootContent_RandomItems : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_RandomItems(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);
			Items = new ImmutableCollection<LootItem>(serializable.Items?.Select(item => LootItem.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }
		public ImmutableCollection<LootItem> Items { get; private set; }


    }
    public partial class LootContent_AllItems : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_AllItems(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Items = new ImmutableCollection<LootItem>(serializable.Items?.Select(item => LootItem.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<LootItem> Items { get; private set; }


    }
    public partial class LootContent_ItemsWithChance : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_ItemsWithChance(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Items = new ImmutableCollection<LootItem>(serializable.Items?.Select(item => LootItem.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<LootItem> Items { get; private set; }


    }
    public partial class LootContent_QuestItem : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_QuestItem(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			QuestItem = loader?.GetQuestItem(new ItemId<QuestItem>(serializable.ItemId)) ?? QuestItem.DefaultValue;
			if (loader != null && QuestItem == null)
			    throw new DatabaseException("MutableObjectTemplate.QuestItem cannot be null - " + serializable.ItemId);
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public QuestItem QuestItem { get; private set; }
		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }
    public partial class LootContent_Ship : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Ship(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			ShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.ItemId)) ?? ShipBuild.DefaultValue;
			if (loader != null && ShipBuild == null)
			    throw new DatabaseException("MutableObjectTemplate.ShipBuild cannot be null - " + serializable.ItemId);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ShipBuild ShipBuild { get; private set; }


    }
    public partial class LootContent_EmptyShip : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_EmptyShip(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Ship = loader?.GetShip(new ItemId<Ship>(serializable.ItemId)) ?? Ship.DefaultValue;
			if (loader != null && Ship == null)
			    throw new DatabaseException("MutableObjectTemplate.Ship cannot be null - " + serializable.ItemId);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public Ship Ship { get; private set; }


    }
    public partial class LootContent_Component : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Component(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Component = loader?.GetComponent(new ItemId<Component>(serializable.ItemId)) ?? Component.DefaultValue;
			if (loader != null && Component == null)
			    throw new DatabaseException("MutableObjectTemplate.Component cannot be null - " + serializable.ItemId);
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public Component Component { get; private set; }
		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }
    public partial class LootContent_Blueprint : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Blueprint(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Blueprint = loader?.GetTechnology(new ItemId<Technology>(serializable.ItemId)) ?? Technology.DefaultValue;
			if (loader != null && Blueprint == null)
			    throw new DatabaseException("MutableObjectTemplate.Blueprint cannot be null - " + serializable.ItemId);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public Technology Blueprint { get; private set; }


    }
    public partial class LootContent_ResearchPoints : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_ResearchPoints(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);
			Factions = RequiredFactions.Create(serializable.Factions, loader);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }
		public RequiredFactions Factions { get; private set; }


    }
    public partial class LootContent_Satellite : LootContent
    {
		partial void OnDataDeserialized(LootContentSerializable serializable, Database.Loader loader);

  		public LootContent_Satellite(LootContentSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Satellite = loader?.GetSatellite(new ItemId<Satellite>(serializable.ItemId)) ?? Satellite.DefaultValue;
			if (loader != null && Satellite == null)
			    throw new DatabaseException("MutableObjectTemplate.Satellite cannot be null - " + serializable.ItemId);
			MinAmount = UnityEngine.Mathf.Clamp(serializable.MinAmount, 0, 999999999);
			MaxAmount = UnityEngine.Mathf.Clamp(serializable.MaxAmount, 0, 999999999);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(ILootContentFactory<T> factory)
        {
            return factory.Create(this);
        }

		public Satellite Satellite { get; private set; }
		public int MinAmount { get; private set; }
		public int MaxAmount { get; private set; }


    }

}

