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

	public interface IBehaviorTreeNodeContent
	{
		void Load(BehaviorTreeNodeSerializable serializable, Database database);
		void Save(ref BehaviorTreeNodeSerializable serializable);
	}

	public partial class BehaviorTreeNode : IDataAdapter
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		private static IBehaviorTreeNodeContent CreateContent(BehaviorNodeType type)
		{
			switch (type)
			{
				case BehaviorNodeType.Success:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.Failure:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.SubTree:
					return new BehaviorTreeNode_SubTree();
				case BehaviorNodeType.Selector:
					return new BehaviorTreeNode_Selector();
				case BehaviorNodeType.Sequence:
					return new BehaviorTreeNode_Sequence();
				case BehaviorNodeType.Parallel:
					return new BehaviorTreeNode_Parallel();
				case BehaviorNodeType.RandomSelector:
					return new BehaviorTreeNode_RandomSelector();
				case BehaviorNodeType.Invertor:
					return new BehaviorTreeNode_Invertor();
				case BehaviorNodeType.Cooldown:
					return new BehaviorTreeNode_Cooldown();
				case BehaviorNodeType.Execute:
					return new BehaviorTreeNode_Execute();
				case BehaviorNodeType.ParallelSequence:
					return new BehaviorTreeNode_ParallelSequence();
				case BehaviorNodeType.PreserveTarget:
					return new BehaviorTreeNode_PreserveTarget();
				case BehaviorNodeType.IfThenElse:
					return new BehaviorTreeNode_IfThenElse();
				case BehaviorNodeType.HasEnoughEnergy:
					return new BehaviorTreeNode_HasEnoughEnergy();
				case BehaviorNodeType.IsLowOnHp:
					return new BehaviorTreeNode_IsLowOnHp();
				case BehaviorNodeType.IsNotControledByPlayer:
					return new BehaviorTreeNode_IsNotControledByPlayer();
				case BehaviorNodeType.HasIncomingThreat:
					return new BehaviorTreeNode_HasIncomingThreat();
				case BehaviorNodeType.HasAdditionalTargets:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.IsFasterThanTarget:
					return new BehaviorTreeNode_IsFasterThanTarget();
				case BehaviorNodeType.HasMainTarget:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MainTargetIsAlly:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MainTargetIsEnemy:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MainTargetLowHp:
					return new BehaviorTreeNode_MainTargetLowHp();
				case BehaviorNodeType.MainTargetWithinAttackRange:
					return new BehaviorTreeNode_MainTargetWithinAttackRange();
				case BehaviorNodeType.HasMothership:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.TargetDistance:
					return new BehaviorTreeNode_TargetDistance();
				case BehaviorNodeType.HasLongerAttackRange:
					return new BehaviorTreeNode_HasLongerAttackRange();
				case BehaviorNodeType.FindEnemy:
					return new BehaviorTreeNode_FindEnemy();
				case BehaviorNodeType.MoveToAttackRange:
					return new BehaviorTreeNode_MoveToAttackRange();
				case BehaviorNodeType.AttackMainTarget:
					return new BehaviorTreeNode_AttackMainTarget();
				case BehaviorNodeType.SelectWeapon:
					return new BehaviorTreeNode_SelectWeapon();
				case BehaviorNodeType.SpawnDrones:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.Ram:
					return new BehaviorTreeNode_Ram();
				case BehaviorNodeType.DetonateShip:
					return new BehaviorTreeNode_DetonateShip();
				case BehaviorNodeType.Vanish:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MaintainAttackRange:
					return new BehaviorTreeNode_MaintainAttackRange();
				case BehaviorNodeType.Wait:
					return new BehaviorTreeNode_Wait();
				case BehaviorNodeType.LookAtTarget:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.LookForAdditionalTargets:
					return new BehaviorTreeNode_LookForAdditionalTargets();
				case BehaviorNodeType.LookForThreats:
					return new BehaviorTreeNode_LookForThreats();
				case BehaviorNodeType.MatchVelocityWithTarget:
					return new BehaviorTreeNode_MatchVelocityWithTarget();
				case BehaviorNodeType.ActivateDevice:
					return new BehaviorTreeNode_ActivateDevice();
				case BehaviorNodeType.RechargeEnergy:
					return new BehaviorTreeNode_RechargeEnergy();
				case BehaviorNodeType.SustainAim:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.ChargeWeapons:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.Chase:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.AvoidThreats:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.SlowDown:
					return new BehaviorTreeNode_SlowDown();
				case BehaviorNodeType.UseRecoil:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.DefendWithFronalShield:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.TrackControllableAmmo:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.KeepDistance:
					return new BehaviorTreeNode_KeepDistance();
				case BehaviorNodeType.ForgetMainTarget:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.EscapeTargetAttackRadius:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.AttackAdditionalTargets:
					return new BehaviorTreeNode_AttackAdditionalTargets();
				case BehaviorNodeType.TargetAllyStarbase:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.TargetEnemyStarbase:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.BypassObstacles:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.AttackTurretTargets:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.HoldHarpoon:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.FindDamagedAlly:
					return new BehaviorTreeNode_FindDamagedAlly();
				case BehaviorNodeType.EnginePropulsionForce:
					return new BehaviorTreeNode_EnginePropulsionForce();
				case BehaviorNodeType.MotherShipRetreated:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MotherShipDestroyed:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.FlyAroundMothership:
					return new BehaviorTreeNode_FlyAroundMothership();
				case BehaviorNodeType.GoBerserk:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.TargetMothership:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MothershipLowHp:
					return new BehaviorTreeNode_MothershipLowHp();
				case BehaviorNodeType.MothershipDistanceExceeded:
					return new BehaviorTreeNode_MothershipDistanceExceeded();
				case BehaviorNodeType.MakeTargetMothership:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.MothershipLowEnergy:
					return new BehaviorTreeNode_MothershipLowEnergy();
				case BehaviorNodeType.MothershipLowShield:
					return new BehaviorTreeNode_MothershipLowShield();
				case BehaviorNodeType.ShowMessage:
					return new BehaviorTreeNode_ShowMessage();
				case BehaviorNodeType.DebugLog:
					return new BehaviorTreeNode_DebugLog();
				case BehaviorNodeType.SetValue:
					return new BehaviorTreeNode_SetValue();
				case BehaviorNodeType.GetValue:
					return new BehaviorTreeNode_GetValue();
				case BehaviorNodeType.SendMessage:
					return new BehaviorTreeNode_SendMessage();
				case BehaviorNodeType.MessageReceived:
					return new BehaviorTreeNode_MessageReceived();
				case BehaviorNodeType.TargetMessageSender:
					return new BehaviorTreeNodeEmptyContent();
				case BehaviorNodeType.SaveTarget:
					return new BehaviorTreeNode_SaveTarget();
				case BehaviorNodeType.LoadTarget:
					return new BehaviorTreeNode_LoadTarget();
				case BehaviorNodeType.HasSavedTarget:
					return new BehaviorTreeNode_HasSavedTarget();
				case BehaviorNodeType.ForgetSavedTarget:
					return new BehaviorTreeNode_ForgetSavedTarget();
				default:
					throw new DatabaseException("BehaviorTreeNode: Invalid content type - " + type);
			}
		}

		public static BehaviorTreeNode Create(BehaviorTreeNodeSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BehaviorTreeNode(serializable, database);
		}

		public BehaviorTreeNode()
		{
			_content = new BehaviorTreeNodeEmptyContent();
		}

		private BehaviorTreeNode(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Type = serializable.Type;
			Requirement.Value = DataModel.BehaviorNodeRequirement.Create(serializable.Requirement, database);
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public BehaviorTreeNodeSerializable Serialize()
		{
			var serializable = new BehaviorTreeNodeSerializable();
			serializable.Nodes = null;
			serializable.Node = null;
			serializable.ItemId = 0;
			serializable.WeaponType = 0;
			serializable.ExecutionMode = 0;
			serializable.Result = false;
			serializable.MinValue = 0.1f;
			serializable.MaxValue = 0.9f;
			serializable.Cooldown = 0f;
			serializable.InRange = false;
			serializable.NoDrones = false;
			serializable.UseSystems = false;
			serializable.DeviceClass = 0;
			serializable.Text = string.Empty;
			serializable.Color = string.Empty;
			_content.Save(ref serializable);
			serializable.Type = Type;
			serializable.Requirement = Requirement.Value?.Serialize();
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
				yield return new Property(this, type.GetField("Requirement"), DataChangedEvent);

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

		private IBehaviorTreeNodeContent _content;
		public BehaviorNodeType Type;
		[TooltipText("The node will not execute and will return FAILURE if the requirement is not met")]
		public ObjectWrapper<BehaviorNodeRequirement> Requirement = new(DataModel.BehaviorNodeRequirement.DefaultValue);

		public static BehaviorTreeNode DefaultValue { get; private set; }
	}

	public class BehaviorTreeNodeEmptyContent : IBehaviorTreeNodeContent
	{
		public void Load(BehaviorTreeNodeSerializable serializable, Database database) {}
		public void Save(ref BehaviorTreeNodeSerializable serializable) {}
	}

	public partial class BehaviorTreeNode_SubTree : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			BehaviourTree = database.GetBehaviorTreeId(serializable.ItemId);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.ItemId = BehaviourTree.Value;
			OnDataSerialized(ref serializable);
		}

		public ItemId<BehaviorTreeModel> BehaviourTree = ItemId<BehaviorTreeModel>.Empty;
	}

	public partial class BehaviorTreeNode_Selector : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
	}

	public partial class BehaviorTreeNode_Sequence : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
	}

	public partial class BehaviorTreeNode_Parallel : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
	}

	public partial class BehaviorTreeNode_RandomSelector : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			serializable.Cooldown = Cooldown.Value;
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_Invertor : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Node.Value = DataModel.BehaviorTreeNode.Create(serializable.Node, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Node = Node.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public ObjectWrapper<BehaviorTreeNode> Node = new(DataModel.BehaviorTreeNode.DefaultValue);
	}

	public partial class BehaviorTreeNode_Cooldown : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Node.Value = DataModel.BehaviorTreeNode.Create(serializable.Node, database);
			ExecutionMode = serializable.ExecutionMode;
			Result = serializable.Result;
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Node = Node.Value?.Serialize();
			serializable.ExecutionMode = ExecutionMode;
			serializable.Result = Result;
			serializable.Cooldown = Cooldown.Value;
			OnDataSerialized(ref serializable);
		}

		public ObjectWrapper<BehaviorTreeNode> Node = new(DataModel.BehaviorTreeNode.DefaultValue);
		public NodeExecutionMode ExecutionMode;
		public bool Result;
		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_Execute : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Node.Value = DataModel.BehaviorTreeNode.Create(serializable.Node, database);
			ExecutionMode = serializable.ExecutionMode;
			Result = serializable.Result;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Node = Node.Value?.Serialize();
			serializable.ExecutionMode = ExecutionMode;
			serializable.Result = Result;
			OnDataSerialized(ref serializable);
		}

		public ObjectWrapper<BehaviorTreeNode> Node = new(DataModel.BehaviorTreeNode.DefaultValue);
		public NodeExecutionMode ExecutionMode;
		public bool Result;
	}

	public partial class BehaviorTreeNode_ParallelSequence : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
	}

	public partial class BehaviorTreeNode_PreserveTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Node.Value = DataModel.BehaviorTreeNode.Create(serializable.Node, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Node = Node.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public ObjectWrapper<BehaviorTreeNode> Node = new(DataModel.BehaviorTreeNode.DefaultValue);
	}

	public partial class BehaviorTreeNode_IfThenElse : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Nodes = serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorTreeNode[] Nodes;
	}

	public partial class BehaviorTreeNode_HasEnoughEnergy : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			FailIfLess = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = FailIfLess.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> FailIfLess = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_IsLowOnHp : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_IsNotControledByPlayer : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0.1f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Cooldown = Cooldown.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0.1f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_HasIncomingThreat : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			TimeToCollision = new NumericValue<float>(serializable.Cooldown, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Cooldown = TimeToCollision.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> TimeToCollision = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_IsFasterThanTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Multiplier = new NumericValue<float>(serializable.MinValue, 1f, 10f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = Multiplier.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Multiplier = new NumericValue<float>(0, 1f, 10f);
	}

	public partial class BehaviorTreeNode_MainTargetLowHp : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_MainTargetWithinAttackRange : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinMaxLerp = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinMaxLerp.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Linear interpolation between shortest and longest weapon ranges")]
		public NumericValue<float> MinMaxLerp = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_TargetDistance : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MaxDistance = new NumericValue<float>(serializable.MaxValue, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MaxValue = MaxDistance.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Max distance. If value is 0, prefefined value will be used (e.g. DroneBay range)")]
		public NumericValue<float> MaxDistance = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_HasLongerAttackRange : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Multiplier = new NumericValue<float>(serializable.MinValue, 1f, 10f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = Multiplier.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Multiplier = new NumericValue<float>(0, 1f, 10f);
	}

	public partial class BehaviorTreeNode_FindEnemy : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinCooldown = new NumericValue<float>(serializable.MinValue, 0.5f, 3.402823E+38f);
			MaxCooldown = new NumericValue<float>(serializable.MaxValue, 0f, 3.402823E+38f);
			InAttackRange = serializable.InRange;
			IgnoreDrones = serializable.NoDrones;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinCooldown.Value;
			serializable.MaxValue = MaxCooldown.Value;
			serializable.InRange = InAttackRange;
			serializable.NoDrones = IgnoreDrones;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinCooldown = new NumericValue<float>(0, 0.5f, 3.402823E+38f);
		public NumericValue<float> MaxCooldown = new NumericValue<float>(0, 0f, 3.402823E+38f);
		public bool InAttackRange;
		public bool IgnoreDrones;
	}

	public partial class BehaviorTreeNode_MoveToAttackRange : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinMaxLerp = new NumericValue<float>(serializable.MinValue, 0f, 1f);
			Multiplier = new NumericValue<float>(serializable.MaxValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinMaxLerp.Value;
			serializable.MaxValue = Multiplier.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Linear interpolation between shortest and longest weapon ranges")]
		public NumericValue<float> MinMaxLerp = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> Multiplier = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_AttackMainTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			NotMoving = serializable.InRange;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.InRange = NotMoving;
			OnDataSerialized(ref serializable);
		}

		public bool NotMoving;
	}

	public partial class BehaviorTreeNode_SelectWeapon : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			WeaponType = serializable.WeaponType;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.WeaponType = WeaponType;
			OnDataSerialized(ref serializable);
		}

		public AiWeaponCategory WeaponType;
	}

	public partial class BehaviorTreeNode_Ram : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			UseShipSystems = serializable.UseSystems;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.UseSystems = UseShipSystems;
			OnDataSerialized(ref serializable);
		}

		public bool UseShipSystems;
	}

	public partial class BehaviorTreeNode_DetonateShip : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			InAttackRange = serializable.InRange;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.InRange = InAttackRange;
			OnDataSerialized(ref serializable);
		}

		public bool InAttackRange;
	}

	public partial class BehaviorTreeNode_MaintainAttackRange : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinMaxLerp = new NumericValue<float>(serializable.MinValue, 0f, 1f);
			Tolerance = new NumericValue<float>(serializable.MaxValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinMaxLerp.Value;
			serializable.MaxValue = Tolerance.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Linear interpolation between shortest and longest weapon ranges")]
		public NumericValue<float> MinMaxLerp = new NumericValue<float>(0, 0f, 1f);
		[TooltipText("A valid distance between ships will be [range*(1-tolerance) .. range]")]
		public NumericValue<float> Tolerance = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_Wait : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0f, 3.402823E+38f);
			ResetIfInterrupted = serializable.InRange;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Cooldown = Cooldown.Value;
			serializable.InRange = ResetIfInterrupted;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0f, 3.402823E+38f);
		public bool ResetIfInterrupted;
	}

	public partial class BehaviorTreeNode_LookForAdditionalTargets : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0.1f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Cooldown = Cooldown.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0.1f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_LookForThreats : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0.1f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Cooldown = Cooldown.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0.1f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_MatchVelocityWithTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Tolerance = new NumericValue<float>(serializable.MaxValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MaxValue = Tolerance.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Acceptable speed deviation")]
		public NumericValue<float> Tolerance = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_ActivateDevice : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			DeviceClass = serializable.DeviceClass;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.DeviceClass = DeviceClass;
			OnDataSerialized(ref serializable);
		}

		public DeviceClass DeviceClass;
	}

	public partial class BehaviorTreeNode_RechargeEnergy : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			FailIfLess = new NumericValue<float>(serializable.MinValue, 0f, 1f);
			RestoreUntil = new NumericValue<float>(serializable.MaxValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = FailIfLess.Value;
			serializable.MaxValue = RestoreUntil.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> FailIfLess = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> RestoreUntil = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_SlowDown : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Tolerance = new NumericValue<float>(serializable.MaxValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MaxValue = Tolerance.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Acceptable speed deviation")]
		public NumericValue<float> Tolerance = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_KeepDistance : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinDistance = new NumericValue<float>(serializable.MinValue, 0f, 1000f);
			MaxDistance = new NumericValue<float>(serializable.MaxValue, 0f, 1000f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinDistance.Value;
			serializable.MaxValue = MaxDistance.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinDistance = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> MaxDistance = new NumericValue<float>(0, 0f, 1000f);
	}

	public partial class BehaviorTreeNode_AttackAdditionalTargets : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			NotMoving = serializable.InRange;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.InRange = NotMoving;
			OnDataSerialized(ref serializable);
		}

		public bool NotMoving;
	}

	public partial class BehaviorTreeNode_FindDamagedAlly : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinCooldown = new NumericValue<float>(serializable.MinValue, 0.5f, 3.402823E+38f);
			MaxCooldown = new NumericValue<float>(serializable.MaxValue, 0f, 3.402823E+38f);
			InAttackRange = serializable.InRange;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinCooldown.Value;
			serializable.MaxValue = MaxCooldown.Value;
			serializable.InRange = InAttackRange;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinCooldown = new NumericValue<float>(0, 0.5f, 3.402823E+38f);
		public NumericValue<float> MaxCooldown = new NumericValue<float>(0, 0f, 3.402823E+38f);
		public bool InAttackRange;
	}

	public partial class BehaviorTreeNode_EnginePropulsionForce : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_FlyAroundMothership : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinDistance = new NumericValue<float>(serializable.MinValue, 0f, 1000f);
			MaxDistance = new NumericValue<float>(serializable.MaxValue, 0f, 1000f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinDistance.Value;
			serializable.MaxValue = MaxDistance.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinDistance = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> MaxDistance = new NumericValue<float>(0, 0f, 1000f);
	}

	public partial class BehaviorTreeNode_MothershipLowHp : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_MothershipDistanceExceeded : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MaxDistance = new NumericValue<float>(serializable.MaxValue, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MaxValue = MaxDistance.Value;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("Max distance. If value is 0, prefefined value will be used (e.g. DroneBay range)")]
		public NumericValue<float> MaxDistance = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorTreeNode_MothershipLowEnergy : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_MothershipLowShield : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			MinValue = new NumericValue<float>(serializable.MinValue, 0f, 1f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.MinValue = MinValue.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> MinValue = new NumericValue<float>(0, 0f, 1f);
	}

	public partial class BehaviorTreeNode_ShowMessage : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Text = serializable.Text;
			Color = Helpers.ColorFromString(serializable.Color);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Text;
			serializable.Color = Helpers.ColorToString(Color);
			OnDataSerialized(ref serializable);
		}

		public string Text;
		public System.Drawing.Color Color;
	}

	public partial class BehaviorTreeNode_DebugLog : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Text = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Text;
			OnDataSerialized(ref serializable);
		}

		public string Text;
	}

	public partial class BehaviorTreeNode_SetValue : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Value = serializable.Result;
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Result = Value;
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public bool Value;
		public string Name;
	}

	public partial class BehaviorTreeNode_GetValue : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_SendMessage : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_MessageReceived : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_SaveTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_LoadTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_HasSavedTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

	public partial class BehaviorTreeNode_ForgetSavedTarget : IBehaviorTreeNodeContent
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorTreeNodeSerializable serializable);

		public void Load(BehaviorTreeNodeSerializable serializable, Database database)
		{
			Name = serializable.Text;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorTreeNodeSerializable serializable)
		{
			serializable.Text = Name;
			OnDataSerialized(ref serializable);
		}

		public string Name;
	}

}

