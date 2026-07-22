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

	public interface INodeContent
	{
		void Load(NodeSerializable serializable, Database database);
		void Save(ref NodeSerializable serializable);
	}

	public partial class Node : IDataAdapter
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		private static INodeContent CreateContent(NodeType type)
		{
			switch (type)
			{
				case NodeType.Undefined:
					return new NodeEmptyContent();
				case NodeType.ComingSoon:
					return new NodeEmptyContent();
				case NodeType.ShowDialog:
					return new Node_ShowDialog();
				case NodeType.OpenShipyard:
					return new Node_OpenShipyard();
				case NodeType.OpenWorkshop:
					return new Node_OpenWorkshop();
				case NodeType.Switch:
					return new Node_Switch();
				case NodeType.Random:
					return new Node_Random();
				case NodeType.Condition:
					return new Node_Condition();
				case NodeType.AttackFleet:
					return new Node_AttackFleet();
				case NodeType.AttackOccupants:
					return new Node_AttackOccupants();
				case NodeType.AttackStarbase:
					return new Node_AttackStarbase();
				case NodeType.DestroyOccupants:
					return new Node_DestroyOccupants();
				case NodeType.SuppressOccupants:
					return new Node_SuppressOccupants();
				case NodeType.Retreat:
					return new Node_Retreat();
				case NodeType.ReceiveItem:
					return new Node_ReceiveItem();
				case NodeType.RemoveItem:
					return new Node_RemoveItem();
				case NodeType.Trade:
					return new Node_Trade();
				case NodeType.CompleteQuest:
					return new NodeEmptyContent();
				case NodeType.FailQuest:
					return new NodeEmptyContent();
				case NodeType.CancelQuest:
					return new NodeEmptyContent();
				case NodeType.StartQuest:
					return new Node_StartQuest();
				case NodeType.SetCharacterRelations:
					return new Node_SetCharacterRelations();
				case NodeType.SetFactionRelations:
					return new Node_SetFactionRelations();
				case NodeType.SetFactionStarbasePower:
					return new Node_SetFactionStarbasePower();
				case NodeType.ChangeCharacterRelations:
					return new Node_ChangeCharacterRelations();
				case NodeType.ChangeFactionRelations:
					return new Node_ChangeFactionRelations();
				case NodeType.ChangeFactionStarbasePower:
					return new Node_ChangeFactionStarbasePower();
				case NodeType.CaptureStarBase:
					return new Node_CaptureStarBase();
				case NodeType.LiberateStarBase:
					return new Node_LiberateStarBase();
				case NodeType.ChangeFaction:
					return new Node_ChangeFaction();
				default:
					throw new DatabaseException("Node: Invalid content type - " + type);
			}
		}

		public static Node Create(NodeSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Node(serializable, database);
		}

		public Node()
		{
			_content = new NodeEmptyContent();
		}

		private Node(NodeSerializable serializable, Database database)
		{
			Id = new NumericValue<int>(serializable.Id, 1, 999999);
			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public NodeSerializable Serialize()
		{
			var serializable = new NodeSerializable();
			serializable.RequiredView = 0;
			serializable.Message = string.Empty;
			serializable.DefaultTransition = 0;
			serializable.FailureTransition = 0;
			serializable.Enemy = 0;
			serializable.Loot = 0;
			serializable.Quest = 0;
			serializable.Character = 0;
			serializable.Faction = 0;
			serializable.Value = 0;
			serializable.Actions = null;
			serializable.Transitions = null;
			_content.Save(ref serializable);
			serializable.Id = Id.Value;
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

				yield return new Property(this, type.GetField("Id"), DataChangedEvent);
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

		private INodeContent _content;
		public NumericValue<int> Id = new NumericValue<int>(0, 1, 999999);
		public NodeType Type;

		public static Node DefaultValue { get; private set; }
	}

	public class NodeEmptyContent : INodeContent
	{
		public void Load(NodeSerializable serializable, Database database) {}
		public void Save(ref NodeSerializable serializable) {}
	}

	public partial class Node_ShowDialog : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			RequiredView = serializable.RequiredView;
			Message = serializable.Message;
			Enemy = database.GetFleetId(serializable.Enemy);
			Loot = database.GetLootId(serializable.Loot);
			Character = database.GetCharacterId(serializable.Character);
			Actions = serializable.Actions?.Select(item => NodeAction.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.RequiredView = RequiredView;
			serializable.Message = Message;
			serializable.Enemy = Enemy.Value;
			serializable.Loot = Loot.Value;
			serializable.Character = Character.Value;
			if (Actions == null || Actions.Length == 0)
			    serializable.Actions = null;
			else
			    serializable.Actions = Actions.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public RequiredViewMode RequiredView;
		public string Message;
		public ItemId<Fleet> Enemy = ItemId<Fleet>.Empty;
		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
		public ItemId<Character> Character = ItemId<Character>.Empty;
		public NodeAction[] Actions;
	}

	public partial class Node_OpenShipyard : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Faction = database.GetFactionId(serializable.Faction);
			Level = new NumericValue<int>(serializable.Value, 0, 10000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Faction = Faction.Value;
			serializable.Value = Level.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
		public NumericValue<int> Level = new NumericValue<int>(0, 0, 10000);
	}

	public partial class Node_OpenWorkshop : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Faction = database.GetFactionId(serializable.Faction);
			Level = new NumericValue<int>(serializable.Value, 0, 10000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Faction = Faction.Value;
			serializable.Value = Level.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
		public NumericValue<int> Level = new NumericValue<int>(0, 0, 10000);
	}

	public partial class Node_Switch : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Message = serializable.Message;
			DefaultTransition = new NumericValue<int>(serializable.DefaultTransition, 0, 999999);
			Transitions = serializable.Transitions?.Select(item => NodeTransition.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.Message = Message;
			serializable.DefaultTransition = DefaultTransition.Value;
			if (Transitions == null || Transitions.Length == 0)
			    serializable.Transitions = null;
			else
			    serializable.Transitions = Transitions.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public string Message;
		public NumericValue<int> DefaultTransition = new NumericValue<int>(0, 0, 999999);
		public NodeTransition[] Transitions;
	}

	public partial class Node_Random : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Message = serializable.Message;
			DefaultTransition = new NumericValue<int>(serializable.DefaultTransition, 0, 999999);
			Transitions = serializable.Transitions?.Select(item => NodeTransition.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.Message = Message;
			serializable.DefaultTransition = DefaultTransition.Value;
			if (Transitions == null || Transitions.Length == 0)
			    serializable.Transitions = null;
			else
			    serializable.Transitions = Transitions.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public string Message;
		public NumericValue<int> DefaultTransition = new NumericValue<int>(0, 0, 999999);
		public NodeTransition[] Transitions;
	}

	public partial class Node_Condition : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Message = serializable.Message;
			Transitions = serializable.Transitions?.Select(item => NodeTransition.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.Message = Message;
			if (Transitions == null || Transitions.Length == 0)
			    serializable.Transitions = null;
			else
			    serializable.Transitions = Transitions.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public string Message;
		public NodeTransition[] Transitions;
	}

	public partial class Node_AttackFleet : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			VictoryTransition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			FailureTransition = new NumericValue<int>(serializable.FailureTransition, 1, 999999);
			Enemy = database.GetFleetId(serializable.Enemy);
			Loot = database.GetLootId(serializable.Loot);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = VictoryTransition.Value;
			serializable.FailureTransition = FailureTransition.Value;
			serializable.Enemy = Enemy.Value;
			serializable.Loot = Loot.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> VictoryTransition = new NumericValue<int>(0, 1, 999999);
		public NumericValue<int> FailureTransition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Fleet> Enemy = ItemId<Fleet>.Empty;
		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
	}

	public partial class Node_AttackOccupants : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			VictoryTransition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			FailureTransition = new NumericValue<int>(serializable.FailureTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = VictoryTransition.Value;
			serializable.FailureTransition = FailureTransition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> VictoryTransition = new NumericValue<int>(0, 1, 999999);
		public NumericValue<int> FailureTransition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_AttackStarbase : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			VictoryTransition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			FailureTransition = new NumericValue<int>(serializable.FailureTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = VictoryTransition.Value;
			serializable.FailureTransition = FailureTransition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> VictoryTransition = new NumericValue<int>(0, 1, 999999);
		public NumericValue<int> FailureTransition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_DestroyOccupants : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_SuppressOccupants : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_Retreat : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_ReceiveItem : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Loot = database.GetLootId(serializable.Loot);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Loot = Loot.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
	}

	public partial class Node_RemoveItem : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Loot = database.GetLootId(serializable.Loot);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Loot = Loot.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
	}

	public partial class Node_Trade : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Loot = database.GetLootId(serializable.Loot);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Loot = Loot.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<LootModel> Loot = ItemId<LootModel>.Empty;
	}

	public partial class Node_StartQuest : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Quest = database.GetQuestId(serializable.Quest);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Quest = Quest.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<QuestModel> Quest = ItemId<QuestModel>.Empty;
	}

	public partial class Node_SetCharacterRelations : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Character = database.GetCharacterId(serializable.Character);
			Value = new NumericValue<int>(serializable.Value, -100, 100);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Character = Character.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Character> Character = ItemId<Character>.Empty;
		public NumericValue<int> Value = new NumericValue<int>(0, -100, 100);
	}

	public partial class Node_SetFactionRelations : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Value = new NumericValue<int>(serializable.Value, -100, 100);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public NumericValue<int> Value = new NumericValue<int>(0, -100, 100);
	}

	public partial class Node_SetFactionStarbasePower : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Value = new NumericValue<int>(serializable.Value, 0, 100000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		[TooltipText("Percentage value")]
		public NumericValue<int> Value = new NumericValue<int>(0, 0, 100000);
	}

	public partial class Node_ChangeCharacterRelations : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Character = database.GetCharacterId(serializable.Character);
			Value = new NumericValue<int>(serializable.Value, -100, 100);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Character = Character.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Character> Character = ItemId<Character>.Empty;
		public NumericValue<int> Value = new NumericValue<int>(0, -100, 100);
	}

	public partial class Node_ChangeFactionRelations : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Value = new NumericValue<int>(serializable.Value, -100, 100);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public NumericValue<int> Value = new NumericValue<int>(0, -100, 100);
	}

	public partial class Node_ChangeFactionStarbasePower : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Value = new NumericValue<int>(serializable.Value, -100000, 100000);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		[TooltipText("Percentage value")]
		public NumericValue<int> Value = new NumericValue<int>(0, -100000, 100000);
	}

	public partial class Node_CaptureStarBase : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_LiberateStarBase : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
	}

	public partial class Node_ChangeFaction : INodeContent
	{
		partial void OnDataDeserialized(NodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeSerializable serializable);

		public void Load(NodeSerializable serializable, Database database)
		{
			Transition = new NumericValue<int>(serializable.DefaultTransition, 1, 999999);
			Faction = database.GetFactionId(serializable.Faction);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref NodeSerializable serializable)
		{
			serializable.DefaultTransition = Transition.Value;
			serializable.Faction = Faction.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<int> Transition = new NumericValue<int>(0, 1, 999999);
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
	}

}

