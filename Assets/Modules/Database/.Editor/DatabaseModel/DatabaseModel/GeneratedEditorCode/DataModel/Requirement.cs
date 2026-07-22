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

	public interface IRequirementContent
	{
		void Load(RequirementSerializable serializable, Database database);
		void Save(ref RequirementSerializable serializable);
	}

	public partial class Requirement : IDataAdapter
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		private static IRequirementContent CreateContent(RequirementType type)
		{
			switch (type)
			{
				case RequirementType.Empty:
					return new RequirementEmptyContent();
				case RequirementType.Any:
					return new Requirement_Any();
				case RequirementType.All:
					return new Requirement_All();
				case RequirementType.None:
					return new Requirement_None();
				case RequirementType.PlayerPosition:
					return new Requirement_PlayerPosition();
				case RequirementType.RandomStarSystem:
					return new Requirement_RandomStarSystem();
				case RequirementType.AggressiveOccupants:
					return new RequirementEmptyContent();
				case RequirementType.QuestCompleted:
					return new Requirement_QuestCompleted();
				case RequirementType.QuestActive:
					return new Requirement_QuestActive();
				case RequirementType.CharacterRelations:
					return new Requirement_CharacterRelations();
				case RequirementType.FactionRelations:
					return new Requirement_FactionRelations();
				case RequirementType.StarbaseCaptured:
					return new RequirementEmptyContent();
				case RequirementType.FactionStarbasePower:
					return new Requirement_FactionStarbasePower();
				case RequirementType.IsHostileFaction:
					return new RequirementEmptyContent();
				case RequirementType.Faction:
					return new Requirement_Faction();
				case RequirementType.HaveQuestItem:
					return new Requirement_HaveQuestItem();
				case RequirementType.HaveItem:
					return new Requirement_HaveItem();
				case RequirementType.HaveItemById:
					return new Requirement_HaveItemById();
				case RequirementType.ComeToOrigin:
					return new Requirement_ComeToOrigin();
				case RequirementType.TimeSinceQuestStart:
					return new Requirement_TimeSinceQuestStart();
				case RequirementType.TimeSinceLastCompletion:
					return new Requirement_TimeSinceLastCompletion();
				default:
					throw new DatabaseException("Requirement: Invalid content type - " + type);
			}
		}

		public static Requirement Create(RequirementSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Requirement(serializable, database);
		}

		public Requirement()
		{
			_content = new RequirementEmptyContent();
		}

		private Requirement(RequirementSerializable serializable, Database database)
		{
			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public RequirementSerializable Serialize()
		{
			var serializable = new RequirementSerializable();
			serializable.ItemId = 0;
			serializable.MinValue = 0;
			serializable.MaxValue = 0;
			serializable.BoolValue = false;
			serializable.Character = 0;
			serializable.Faction = 0;
			serializable.Loot = null;
			serializable.Requirements = null;
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

		private IRequirementContent _content;
		public RequirementType Type;

		public static Requirement DefaultValue { get; private set; }
	}

	public class RequirementEmptyContent : IRequirementContent
	{
		public void Load(RequirementSerializable serializable, Database database) {}
		public void Save(ref RequirementSerializable serializable) {}
	}

	public partial class Requirement_Any : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => Requirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public Requirement[] Requirements;
	}

	public partial class Requirement_All : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => Requirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public Requirement[] Requirements;
	}

	public partial class Requirement_None : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => Requirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public Requirement[] Requirements;
	}

	public partial class Requirement_PlayerPosition : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			MinValue = new NumericValue<int>(serializable.MinValue, 0, 10000);
			MaxValue = new NumericValue<int>(serializable.MaxValue, 0, 10000);
			AllowUnsafeStars = serializable.BoolValue;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			serializable.MaxValue = MaxValue.Value;
			serializable.BoolValue = AllowUnsafeStars;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinValue = new NumericValue<int>(0, 0, 10000);
		public NumericValue<int> MaxValue = new NumericValue<int>(0, 0, 10000);
		public bool AllowUnsafeStars;
	}

	public partial class Requirement_RandomStarSystem : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			MinValue = new NumericValue<int>(serializable.MinValue, 0, 10000);
			MaxValue = new NumericValue<int>(serializable.MaxValue, 0, 10000);
			AllowUnsafeStars = serializable.BoolValue;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			serializable.MaxValue = MaxValue.Value;
			serializable.BoolValue = AllowUnsafeStars;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinValue = new NumericValue<int>(0, 0, 10000);
		public NumericValue<int> MaxValue = new NumericValue<int>(0, 0, 10000);
		public bool AllowUnsafeStars;
	}

	public partial class Requirement_QuestCompleted : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Quest = database.GetQuestId(serializable.ItemId);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.ItemId = Quest.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<QuestModel> Quest = ItemId<QuestModel>.Empty;
	}

	public partial class Requirement_QuestActive : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Quest = database.GetQuestId(serializable.ItemId);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.ItemId = Quest.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<QuestModel> Quest = ItemId<QuestModel>.Empty;
	}

	public partial class Requirement_CharacterRelations : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			MinValue = new NumericValue<int>(serializable.MinValue, -100, 100);
			MaxValue = new NumericValue<int>(serializable.MaxValue, -100, 100);
			Character = database.GetCharacterId(serializable.Character);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			serializable.MaxValue = MaxValue.Value;
			serializable.Character = Character.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinValue = new NumericValue<int>(0, -100, 100);
		public NumericValue<int> MaxValue = new NumericValue<int>(0, -100, 100);
		public ItemId<Character> Character = ItemId<Character>.Empty;
	}

	public partial class Requirement_FactionRelations : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			MinValue = new NumericValue<int>(serializable.MinValue, -100, 100);
			MaxValue = new NumericValue<int>(serializable.MaxValue, -100, 100);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			serializable.MaxValue = MaxValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> MinValue = new NumericValue<int>(0, -100, 100);
		public NumericValue<int> MaxValue = new NumericValue<int>(0, -100, 100);
	}

	public partial class Requirement_FactionStarbasePower : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			MinValue = new NumericValue<int>(serializable.MinValue, 0, 100000);
			MaxValue = new NumericValue<int>(serializable.MaxValue, 0, 100000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			serializable.MaxValue = MaxValue.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Percentage value")]
		public NumericValue<int> MinValue = new NumericValue<int>(0, 0, 100000);
		[TooltipText("Percentage value")]
		public NumericValue<int> MaxValue = new NumericValue<int>(0, 0, 100000);
	}

	public partial class Requirement_Faction : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Faction = database.GetFactionId(serializable.Faction);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.Faction = Faction.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
	}

	public partial class Requirement_HaveQuestItem : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			QuestItem = database.GetQuestItemId(serializable.ItemId);
			Amount = new NumericValue<int>(serializable.MinValue, 1, 1000000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.ItemId = QuestItem.Value;
			serializable.MinValue = Amount.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<QuestItem> QuestItem = ItemId<QuestItem>.Empty;
		public NumericValue<int> Amount = new NumericValue<int>(0, 1, 1000000);
	}

	public partial class Requirement_HaveItem : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Loot.Value = DataModel.LootContent.Create(serializable.Loot, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.Loot = Loot.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public ObjectWrapper<LootContent> Loot = new(DataModel.LootContent.DefaultValue);
	}

	public partial class Requirement_HaveItemById : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Loot = database.GetLootId(serializable.ItemId);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.ItemId = Loot.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
	}

	public partial class Requirement_ComeToOrigin : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			AllowUnsafeStars = serializable.BoolValue;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.BoolValue = AllowUnsafeStars;
			OnDataSerialized(ref serializable);
		}

		public bool AllowUnsafeStars;
	}

	public partial class Requirement_TimeSinceQuestStart : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Minutes = new NumericValue<int>(serializable.MinValue, 0, 999999);
			Hours = new NumericValue<int>(serializable.MaxValue, 0, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = Minutes.Value;
			serializable.MaxValue = Hours.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Minutes = new NumericValue<int>(0, 0, 999999);
		public NumericValue<int> Hours = new NumericValue<int>(0, 0, 999999);
	}

	public partial class Requirement_TimeSinceLastCompletion : IRequirementContent
	{
		partial void OnDataDeserialized(RequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref RequirementSerializable serializable);

		public void Load(RequirementSerializable serializable, Database database)
		{
			Minutes = new NumericValue<int>(serializable.MinValue, 0, 999999);
			Hours = new NumericValue<int>(serializable.MaxValue, 0, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref RequirementSerializable serializable)
		{
			serializable.MinValue = Minutes.Value;
			serializable.MaxValue = Hours.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Minutes = new NumericValue<int>(0, 0, 999999);
		public NumericValue<int> Hours = new NumericValue<int>(0, 0, 999999);
	}

}

