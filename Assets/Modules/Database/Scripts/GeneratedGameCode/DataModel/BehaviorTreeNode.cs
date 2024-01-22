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
	public abstract partial class BehaviorTreeNode
	{
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

		public static BehaviorTreeNode Create(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case BehaviorNodeType.Undefined:
					return new BehaviorTreeNode_Undefined(serializable, loader);
				case BehaviorNodeType.SubTree:
					return new BehaviorTreeNode_SubTree(serializable, loader);
				case BehaviorNodeType.Selector:
					return new BehaviorTreeNode_Selector(serializable, loader);
				case BehaviorNodeType.Sequence:
					return new BehaviorTreeNode_Sequence(serializable, loader);
				case BehaviorNodeType.Parallel:
					return new BehaviorTreeNode_Parallel(serializable, loader);
				case BehaviorNodeType.RandomSelector:
					return new BehaviorTreeNode_RandomSelector(serializable, loader);
				case BehaviorNodeType.Invertor:
					return new BehaviorTreeNode_Invertor(serializable, loader);
				case BehaviorNodeType.ConstantResult:
					return new BehaviorTreeNode_ConstantResult(serializable, loader);
				case BehaviorNodeType.CompleteOnce:
					return new BehaviorTreeNode_CompleteOnce(serializable, loader);
				case BehaviorNodeType.RandomExecutor:
					return new BehaviorTreeNode_RandomExecutor(serializable, loader);
				case BehaviorNodeType.HaveEnoughEnergy:
					return new BehaviorTreeNode_HaveEnoughEnergy(serializable, loader);
				case BehaviorNodeType.IsLowOnHp:
					return new BehaviorTreeNode_IsLowOnHp(serializable, loader);
				case BehaviorNodeType.IsControledByPlayer:
					return new BehaviorTreeNode_IsControledByPlayer(serializable, loader);
				case BehaviorNodeType.HaveIncomingThreat:
					return new BehaviorTreeNode_HaveIncomingThreat(serializable, loader);
				case BehaviorNodeType.FindEnemy:
					return new BehaviorTreeNode_FindEnemy(serializable, loader);
				case BehaviorNodeType.MoveToAttackRange:
					return new BehaviorTreeNode_MoveToAttackRange(serializable, loader);
				case BehaviorNodeType.Attack:
					return new BehaviorTreeNode_Attack(serializable, loader);
				case BehaviorNodeType.SelectWeapon:
					return new BehaviorTreeNode_SelectWeapon(serializable, loader);
				case BehaviorNodeType.SpawnDrones:
					return new BehaviorTreeNode_SpawnDrones(serializable, loader);
				case BehaviorNodeType.Ram:
					return new BehaviorTreeNode_Ram(serializable, loader);
				case BehaviorNodeType.DetonateShip:
					return new BehaviorTreeNode_DetonateShip(serializable, loader);
				case BehaviorNodeType.Vanish:
					return new BehaviorTreeNode_Vanish(serializable, loader);
				case BehaviorNodeType.MaintainAttackRange:
					return new BehaviorTreeNode_MaintainAttackRange(serializable, loader);
				case BehaviorNodeType.Wait:
					return new BehaviorTreeNode_Wait(serializable, loader);
				case BehaviorNodeType.LookAtTarget:
					return new BehaviorTreeNode_LookAtTarget(serializable, loader);
				case BehaviorNodeType.LookForSecondaryTargets:
					return new BehaviorTreeNode_LookForSecondaryTargets(serializable, loader);
				case BehaviorNodeType.LookForThreats:
					return new BehaviorTreeNode_LookForThreats(serializable, loader);
				case BehaviorNodeType.AttackSecondaryTargets:
					return new BehaviorTreeNode_AttackSecondaryTargets(serializable, loader);
				case BehaviorNodeType.ActivateDevice:
					return new BehaviorTreeNode_ActivateDevice(serializable, loader);
				case BehaviorNodeType.RechargeEnergy:
					return new BehaviorTreeNode_RechargeEnergy(serializable, loader);
				case BehaviorNodeType.SustainAim:
					return new BehaviorTreeNode_SustainAim(serializable, loader);
				case BehaviorNodeType.ChargeWeapons:
					return new BehaviorTreeNode_ChargeWeapons(serializable, loader);
				case BehaviorNodeType.Chase:
					return new BehaviorTreeNode_Chase(serializable, loader);
				case BehaviorNodeType.AvoidThreats:
					return new BehaviorTreeNode_AvoidThreats(serializable, loader);
				case BehaviorNodeType.Stop:
					return new BehaviorTreeNode_Stop(serializable, loader);
				case BehaviorNodeType.UseRecoil:
					return new BehaviorTreeNode_UseRecoil(serializable, loader);
				case BehaviorNodeType.DefendWithFronalShield:
					return new BehaviorTreeNode_DefendWithFronalShield(serializable, loader);
				case BehaviorNodeType.IsWithinAttackRange:
					return new BehaviorTreeNode_IsWithinAttackRange(serializable, loader);
				case BehaviorNodeType.MotherShipRetreated:
					return new BehaviorTreeNode_MotherShipRetreated(serializable, loader);
				case BehaviorNodeType.MotherShipDestroyed:
					return new BehaviorTreeNode_MotherShipDestroyed(serializable, loader);
				case BehaviorNodeType.FlyAroundMothership:
					return new BehaviorTreeNode_FlyAroundMothership(serializable, loader);
				case BehaviorNodeType.GoBerserk:
					return new BehaviorTreeNode_GoBerserk(serializable, loader);
				case BehaviorNodeType.TargetMothership:
					return new BehaviorTreeNode_TargetMothership(serializable, loader);
				case BehaviorNodeType.MothershipLowHp:
					return new BehaviorTreeNode_MothershipLowHp(serializable, loader);
				case BehaviorNodeType.ShowMessage:
					return new BehaviorTreeNode_ShowMessage(serializable, loader);
				case BehaviorNodeType.DebugLog:
					return new BehaviorTreeNode_DebugLog(serializable, loader);
				default:
                    throw new DatabaseException("BehaviorTreeNode: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(IBehaviorTreeNodeFactory<T> factory);

		protected BehaviorTreeNode(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;
			Requirement = BehaviorNodeRequirement.Create(serializable.Requirement, loader);

			OnDataDeserialized(serializable, loader);
		}

		public BehaviorNodeType Type { get; private set; }
		public BehaviorNodeRequirement Requirement { get; private set; }

		public static BehaviorTreeNode DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface IBehaviorTreeNodeFactory<T>
    {
	    T Create(BehaviorTreeNode_Undefined content);
	    T Create(BehaviorTreeNode_SubTree content);
	    T Create(BehaviorTreeNode_Selector content);
	    T Create(BehaviorTreeNode_Sequence content);
	    T Create(BehaviorTreeNode_Parallel content);
	    T Create(BehaviorTreeNode_RandomSelector content);
	    T Create(BehaviorTreeNode_Invertor content);
	    T Create(BehaviorTreeNode_ConstantResult content);
	    T Create(BehaviorTreeNode_CompleteOnce content);
	    T Create(BehaviorTreeNode_RandomExecutor content);
	    T Create(BehaviorTreeNode_HaveEnoughEnergy content);
	    T Create(BehaviorTreeNode_IsLowOnHp content);
	    T Create(BehaviorTreeNode_IsControledByPlayer content);
	    T Create(BehaviorTreeNode_HaveIncomingThreat content);
	    T Create(BehaviorTreeNode_FindEnemy content);
	    T Create(BehaviorTreeNode_MoveToAttackRange content);
	    T Create(BehaviorTreeNode_Attack content);
	    T Create(BehaviorTreeNode_SelectWeapon content);
	    T Create(BehaviorTreeNode_SpawnDrones content);
	    T Create(BehaviorTreeNode_Ram content);
	    T Create(BehaviorTreeNode_DetonateShip content);
	    T Create(BehaviorTreeNode_Vanish content);
	    T Create(BehaviorTreeNode_MaintainAttackRange content);
	    T Create(BehaviorTreeNode_Wait content);
	    T Create(BehaviorTreeNode_LookAtTarget content);
	    T Create(BehaviorTreeNode_LookForSecondaryTargets content);
	    T Create(BehaviorTreeNode_LookForThreats content);
	    T Create(BehaviorTreeNode_AttackSecondaryTargets content);
	    T Create(BehaviorTreeNode_ActivateDevice content);
	    T Create(BehaviorTreeNode_RechargeEnergy content);
	    T Create(BehaviorTreeNode_SustainAim content);
	    T Create(BehaviorTreeNode_ChargeWeapons content);
	    T Create(BehaviorTreeNode_Chase content);
	    T Create(BehaviorTreeNode_AvoidThreats content);
	    T Create(BehaviorTreeNode_Stop content);
	    T Create(BehaviorTreeNode_UseRecoil content);
	    T Create(BehaviorTreeNode_DefendWithFronalShield content);
	    T Create(BehaviorTreeNode_IsWithinAttackRange content);
	    T Create(BehaviorTreeNode_MotherShipRetreated content);
	    T Create(BehaviorTreeNode_MotherShipDestroyed content);
	    T Create(BehaviorTreeNode_FlyAroundMothership content);
	    T Create(BehaviorTreeNode_GoBerserk content);
	    T Create(BehaviorTreeNode_TargetMothership content);
	    T Create(BehaviorTreeNode_MothershipLowHp content);
	    T Create(BehaviorTreeNode_ShowMessage content);
	    T Create(BehaviorTreeNode_DebugLog content);
    }

    public partial class BehaviorTreeNode_Undefined : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Undefined(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_SubTree : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_SubTree(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			BehaviourTree = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.ItemId)) ?? BehaviorTreeModel.DefaultValue;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public BehaviorTreeModel BehaviourTree { get; private set; }
    }
    public partial class BehaviorTreeNode_Selector : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Selector(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Nodes = new ImmutableCollection<BehaviorTreeNode>(serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorTreeNode> Nodes { get; private set; }
    }
    public partial class BehaviorTreeNode_Sequence : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Sequence(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Nodes = new ImmutableCollection<BehaviorTreeNode>(serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorTreeNode> Nodes { get; private set; }
    }
    public partial class BehaviorTreeNode_Parallel : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Parallel(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Nodes = new ImmutableCollection<BehaviorTreeNode>(serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorTreeNode> Nodes { get; private set; }
    }
    public partial class BehaviorTreeNode_RandomSelector : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_RandomSelector(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Nodes = new ImmutableCollection<BehaviorTreeNode>(serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, loader)));
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorTreeNode> Nodes { get; private set; }
		public float Cooldown { get; private set; }
    }
    public partial class BehaviorTreeNode_Invertor : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Invertor(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Node = BehaviorTreeNode.Create(serializable.Node, loader);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public BehaviorTreeNode Node { get; private set; }
    }
    public partial class BehaviorTreeNode_ConstantResult : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_ConstantResult(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Node = BehaviorTreeNode.Create(serializable.Node, loader);
			Result = serializable.Result;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public BehaviorTreeNode Node { get; private set; }
		public bool Result { get; private set; }
    }
    public partial class BehaviorTreeNode_CompleteOnce : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_CompleteOnce(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Node = BehaviorTreeNode.Create(serializable.Node, loader);
			Result = serializable.Result;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public BehaviorTreeNode Node { get; private set; }
		public bool Result { get; private set; }
    }
    public partial class BehaviorTreeNode_RandomExecutor : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_RandomExecutor(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Nodes = new ImmutableCollection<BehaviorTreeNode>(serializable.Nodes?.Select(item => BehaviorTreeNode.Create(item, loader)));
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorTreeNode> Nodes { get; private set; }
		public float Cooldown { get; private set; }
    }
    public partial class BehaviorTreeNode_HaveEnoughEnergy : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_HaveEnoughEnergy(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			FailIfLess = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float FailIfLess { get; private set; }
    }
    public partial class BehaviorTreeNode_IsLowOnHp : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_IsLowOnHp(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinValue = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinValue { get; private set; }
    }
    public partial class BehaviorTreeNode_IsControledByPlayer : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_IsControledByPlayer(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_HaveIncomingThreat : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_HaveIncomingThreat(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			TimeToCollision = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float TimeToCollision { get; private set; }
    }
    public partial class BehaviorTreeNode_FindEnemy : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_FindEnemy(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinCooldown = UnityEngine.Mathf.Clamp(serializable.MinValue, 0.5f, 3.402823E+38f);
			MaxCooldown = UnityEngine.Mathf.Clamp(serializable.MaxValue, 0f, 3.402823E+38f);
			InAttackRange = serializable.InRange;
			IgnoreDrones = serializable.NoDrones;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinCooldown { get; private set; }
		public float MaxCooldown { get; private set; }
		public bool InAttackRange { get; private set; }
		public bool IgnoreDrones { get; private set; }
    }
    public partial class BehaviorTreeNode_MoveToAttackRange : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_MoveToAttackRange(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinMaxLerp = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);
			Multiplier = UnityEngine.Mathf.Clamp(serializable.MaxValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinMaxLerp { get; private set; }
		public float Multiplier { get; private set; }
    }
    public partial class BehaviorTreeNode_Attack : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Attack(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_SelectWeapon : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_SelectWeapon(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			WeaponType = serializable.WeaponType;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public AiWeaponCategory WeaponType { get; private set; }
    }
    public partial class BehaviorTreeNode_SpawnDrones : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_SpawnDrones(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_Ram : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Ram(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			UseShipSystems = serializable.UseSystems;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public bool UseShipSystems { get; private set; }
    }
    public partial class BehaviorTreeNode_DetonateShip : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_DetonateShip(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			InAttackRange = serializable.InRange;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public bool InAttackRange { get; private set; }
    }
    public partial class BehaviorTreeNode_Vanish : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Vanish(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_MaintainAttackRange : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_MaintainAttackRange(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinMaxLerp = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);
			Tolerance = UnityEngine.Mathf.Clamp(serializable.MaxValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinMaxLerp { get; private set; }
		public float Tolerance { get; private set; }
    }
    public partial class BehaviorTreeNode_Wait : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Wait(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 3.402823E+38f);
			ResetIfInterrupted = serializable.InRange;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Cooldown { get; private set; }
		public bool ResetIfInterrupted { get; private set; }
    }
    public partial class BehaviorTreeNode_LookAtTarget : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_LookAtTarget(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_LookForSecondaryTargets : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_LookForSecondaryTargets(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0.1f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Cooldown { get; private set; }
    }
    public partial class BehaviorTreeNode_LookForThreats : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_LookForThreats(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0.1f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Cooldown { get; private set; }
    }
    public partial class BehaviorTreeNode_AttackSecondaryTargets : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_AttackSecondaryTargets(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_ActivateDevice : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_ActivateDevice(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			DeviceClass = serializable.DeviceClass;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public DeviceClass DeviceClass { get; private set; }
    }
    public partial class BehaviorTreeNode_RechargeEnergy : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_RechargeEnergy(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			FailIfLess = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);
			RestoreUntil = UnityEngine.Mathf.Clamp(serializable.MaxValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float FailIfLess { get; private set; }
		public float RestoreUntil { get; private set; }
    }
    public partial class BehaviorTreeNode_SustainAim : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_SustainAim(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_ChargeWeapons : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_ChargeWeapons(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_Chase : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Chase(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_AvoidThreats : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_AvoidThreats(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_Stop : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_Stop(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_UseRecoil : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_UseRecoil(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_DefendWithFronalShield : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_DefendWithFronalShield(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_IsWithinAttackRange : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_IsWithinAttackRange(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinMaxLerp = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinMaxLerp { get; private set; }
    }
    public partial class BehaviorTreeNode_MotherShipRetreated : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_MotherShipRetreated(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_MotherShipDestroyed : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_MotherShipDestroyed(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_FlyAroundMothership : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_FlyAroundMothership(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinDistance = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 10f);
			MaxDistance = UnityEngine.Mathf.Clamp(serializable.MaxValue, 0f, 10f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinDistance { get; private set; }
		public float MaxDistance { get; private set; }
    }
    public partial class BehaviorTreeNode_GoBerserk : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_GoBerserk(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_TargetMothership : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_TargetMothership(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorTreeNode_MothershipLowHp : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_MothershipLowHp(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			MinValue = UnityEngine.Mathf.Clamp(serializable.MinValue, 0f, 1f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float MinValue { get; private set; }
    }
    public partial class BehaviorTreeNode_ShowMessage : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_ShowMessage(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Text = serializable.Text;
			Color = new ColorData(serializable.Color);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public string Text { get; private set; }
		public ColorData Color { get; private set; }
    }
    public partial class BehaviorTreeNode_DebugLog : BehaviorTreeNode
    {
		partial void OnDataDeserialized(BehaviorTreeNodeSerializable serializable, Database.Loader loader);

  		public BehaviorTreeNode_DebugLog(BehaviorTreeNodeSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Text = serializable.Text;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorTreeNodeFactory<T> factory)
        {
            return factory.Create(this);
        }

		public string Text { get; private set; }
    }

}

