using GameDatabase.DataModel;
using Services.Localization;
using Combat.Ai.BehaviorTree.Nodes;
using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class NodeBuilder
	{
		private readonly NodeFactory _factory;
		private readonly RequirementChecker _requirementChecker;
		private readonly ILocalization _localization;
		private readonly MessageHub _messageHub;
		private readonly AiSettings _settings;
		private readonly IShip _ship;
		private readonly IdentifiersMap _identifiersMap = new();

		public NodeBuilder(IShip ship, AiSettings settings, MessageHub messageHub, ILocalization localization)
		{
			_ship = ship;
			_settings = settings;
			_messageHub = messageHub;
			_localization = localization;
			_requirementChecker = new RequirementChecker(ship, settings);
			_factory = new NodeFactory(this);
		}

		public INode Build(BehaviorTreeNode model)
		{
			if (model == null || !model.Requirement.Create(_requirementChecker)) 
				return EmptyNode.Failure;

			return model.Create(_factory);
		}

		private class NodeFactory : IBehaviorTreeNodeFactory<INode>
		{
			private readonly NodeBuilder _builder;
			private System.Random _random;
			private IShip Ship => _builder._ship;
			private AiSettings Settings => _builder._settings;
			private MessageHub MessageHub => _builder._messageHub;
			private ILocalization Localization => _builder._localization;
			private System.Random Random => _random ??= new();
			private float RandomValue => (float)Random.NextDouble();
			private float RandomRange(float min, float max) => min + (max - min) * RandomValue;

			private int StrinToId(string name) => _builder._identifiersMap.GetMessageId(name);

			public NodeFactory(NodeBuilder builder)
			{
				_builder = builder;
				_random = new();
			}

			public INode Create(BehaviorTreeNode_Undefined content) => EmptyNode.Success;
			public INode Create(BehaviorTreeNode_SubTree content) => _builder.Build(content.BehaviourTree?.RootNode);
			public INode Create(BehaviorTreeNode_Invertor content) => InvertorNode.Create(_builder.Build(content.Node));
			public INode Create(BehaviorTreeNode_Cooldown content) => CooldownNode.Create(_builder.Build(content.Node),
				content.ExecutionMode, content.Cooldown, content.Result ? NodeState.Success : NodeState.Failure);
			public INode Create(BehaviorTreeNode_Execute content) => ExecuteNode.Create(_builder.Build(content.Node),
				content.ExecutionMode, content.Result ? NodeState.Success : NodeState.Failure);
            public INode Create(BehaviorTreeNode_PreserveTarget content) => PreserveTargetNode.Create(_builder.Build(content.Node));

            public INode Create(BehaviorTreeNode_Selector content)
			{
				var selector = new SelectorNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Failure) continue;
					selector.Nodes.Add(node);
					if (node == EmptyNode.Success) break;
				}

				if (selector.Nodes.Count == 0) return EmptyNode.Failure;
				if (selector.Nodes.Count == 1) return selector.Nodes[0];
				return selector;
			}

			public INode Create(BehaviorTreeNode_Sequence content)
			{
				var sequence = new SequenceNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Success) continue;
					sequence.Nodes.Add(node);
					if (node == EmptyNode.Failure) break;
				}

				if (sequence.Nodes.Count == 0) return EmptyNode.Success;
				if (sequence.Nodes.Count == 1) return sequence.Nodes[0];
				return sequence;
			}

			public INode Create(BehaviorTreeNode_ParallelSequence content)
			{
				var sequence = new ParallelSequenceNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Success) continue;
					sequence.Nodes.Add(node);
					if (node == EmptyNode.Failure) break;
				}

				if (sequence.Nodes.Count == 0) return EmptyNode.Success;
				if (sequence.Nodes.Count == 1) return sequence.Nodes[0];
				return sequence;
			}

			public INode Create(BehaviorTreeNode_Parallel content)
			{
				var parallel = new ParallelNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Failure) continue;
					parallel.Nodes.Add(node);
				}

				if (parallel.Nodes.Count == 0) return EmptyNode.Failure;
				if (parallel.Nodes.Count == 1) return parallel.Nodes[0];
				return parallel;
			}

			public INode Create(BehaviorTreeNode_RandomSelector content)
			{
				if (content.Nodes.Count == 0) return EmptyNode.Failure;
				if (content.Nodes.Count == 1) return _builder.Build(content.Nodes[0]);

				var random = new RandomSelectorNode(content.Cooldown);
				foreach (var child in content.Nodes)
					random.Add(_builder.Build(child));

				return random;
			}

			public INode Create(BehaviorTreeNode_FindEnemy content)
			{
                var isDrone = Ship.Type.Class == Component.Unit.Classification.UnitClass.Drone && Settings.LimitedRange > 0;

                // TODO: make separate node or additional option for defensive drones
                var isDefensiveDrone = isDrone && content.InAttackRange;

                if (content.InAttackRange && !isDrone)
                    return new FindEnemyInAttackRange(content.MinCooldown, content.MaxCooldown, content.IgnoreDrones);

                if (Settings.LimitedRange > 0)
                {
                    var range = isDefensiveDrone ? Settings.LimitedRange / 4 : Settings.LimitedRange;
                    return new FindEnemyNearMothership(content.MinCooldown, content.MaxCooldown, range, content.IgnoreDrones);
                }

                return new FindNearestEnemy(content.MinCooldown, content.MaxCooldown, content.IgnoreDrones);
            }

            public INode Create(BehaviorTreeNode_MoveToAttackRange content) => new MoveToAttackRange(content.MinMaxLerp, content.Multiplier > 0 ? content.Multiplier : 1.0f);
			public INode Create(BehaviorTreeNode_FlyAroundMothership content) => new FlyAroundMothership(RandomRange(content.MinDistance, content.MaxDistance));
			public INode Create(BehaviorTreeNode_HasEnoughEnergy content) => new HaveEnoughEnergy(content.FailIfLess);
			public INode Create(BehaviorTreeNode_SelectWeapon content) => SelectWeaponNode.Create(Ship, content.WeaponType);
			public INode Create(BehaviorTreeNode_SpawnDrones content) => new SpawnDronesNode(Ship);
			public INode Create(BehaviorTreeNode_Ram content) => new RamNode(Ship, content.UseShipSystems);
			public INode Create(BehaviorTreeNode_DetonateShip content) => new DetonateShipNode(Ship, content.InAttackRange);
			public INode Create(BehaviorTreeNode_IsLowOnHp content) => new IsLowOnHp(content.MinValue);
			public INode Create(BehaviorTreeNode_Vanish content) => new VanishNode();
			public INode Create(BehaviorTreeNode_MotherShipRetreated content) => new MothershipRetreated();
			public INode Create(BehaviorTreeNode_MotherShipDestroyed content) => new MothershipDestroyed();
			public INode Create(BehaviorTreeNode_GoBerserk content) => new GoBerserkNode();
			public INode Create(BehaviorTreeNode_TargetMothership content) => new TargetMothershipNode();
			public INode Create(BehaviorTreeNode_MaintainAttackRange content) => new MaintainAttackRange(content.MinMaxLerp, content.Tolerance);
			public INode Create(BehaviorTreeNode_MainTargetWithinAttackRange content) => new IsWithinAttackRange(content.MinMaxLerp);
			public INode Create(BehaviorTreeNode_MothershipLowHp content) => new MothershipLowHpNode(content.MinValue);
			public INode Create(BehaviorTreeNode_IsControledByPlayer content) => new IsPlayerControlled();
			public INode Create(BehaviorTreeNode_Wait content) => new WaitNode(content.Cooldown, content.ResetIfInterrupted);
			public INode Create(BehaviorTreeNode_LookAtTarget content) => new LookAtTargetNode();
			public INode Create(BehaviorTreeNode_LookForAdditionalTargets content) => new UpdateSecondaryTargets(content.Cooldown);
			public INode Create(BehaviorTreeNode_LookForThreats content) => new UpdateThreats(content.Cooldown);
			public INode Create(BehaviorTreeNode_ActivateDevice content) => ActivateDeviceNode.Create(Ship, content.DeviceClass);
			public INode Create(BehaviorTreeNode_RechargeEnergy content) => new RechargeEnergy(content.FailIfLess, content.RestoreUntil);
			public INode Create(BehaviorTreeNode_SustainAim content) => new SustainAimNode(false);
			public INode Create(BehaviorTreeNode_ChargeWeapons content) => ChargeWeaponsNode.Create(Ship);
			public INode Create(BehaviorTreeNode_Chase content) => new ChaseNode();
            public INode Create(BehaviorTreeNode_AvoidThreats content) => new AvoidThreatsNode();
            public INode Create(BehaviorTreeNode_BypassObstacles content) => new BypassObstaclesNode();
            public INode Create(BehaviorTreeNode_HasIncomingThreat content) => new HasIncomingThreatNode(content.TimeToCollision);
			public INode Create(BehaviorTreeNode_SlowDown content) => new SlowDownNode(content.Tolerance);
			public INode Create(BehaviorTreeNode_UseRecoil content) => RecoilNode.Create(Ship);
			public INode Create(BehaviorTreeNode_DefendWithFronalShield content) => FrontalShieldNode.Create(Ship);
			public INode Create(BehaviorTreeNode_TrackControllableAmmo content) => TrackControllableAmmo.Create(Ship, true);
            public INode Create(BehaviorTreeNode_MothershipDistanceExceeded content) => MothershipRangeExceeded.Create(content.MaxDistance > 0 ? content.MaxDistance : Settings.LimitedRange);
            public INode Create(BehaviorTreeNode_IsFasterThanTarget content) => new IsFaterThanTarget(content.Multiplier);
			public INode Create(BehaviorTreeNode_MatchVelocityWithTarget content) => new MatchVelocityNode(content.Tolerance);
			public INode Create(BehaviorTreeNode_KeepDistance content) => new KeepDistanceNode(content.MinDistance, content.MaxDistance);
			public INode Create(BehaviorTreeNode_HasMainTarget content) => new HasTargetNode();
			public INode Create(BehaviorTreeNode_MainTargetIsAlly content) => new TargetIsAllyNode();
			public INode Create(BehaviorTreeNode_MainTargetIsEnemy content) => new TargetIsEnemyNode();
			public INode Create(BehaviorTreeNode_MainTargetLowHp content) => new TargetLowHpNode(content.MinValue);
			public INode Create(BehaviorTreeNode_EnginePropulsionForce content) => new PropulsionForceNode(content.MinValue);
			public INode Create(BehaviorTreeNode_ShowMessage content) => new ShowMessageNode(Localization.Localize(content.Text), content.Color);
			public INode Create(BehaviorTreeNode_DebugLog content) => new DebugLogNode(content.Text);
			public INode Create(BehaviorTreeNode_SetValue content) => new SetValueNode(StrinToId(content.Name), content.Value);
			public INode Create(BehaviorTreeNode_GetValue content) => new GetValueNode(StrinToId(content.Name));
			public INode Create(BehaviorTreeNode_SendMessage content) => new SendMessageNode(MessageHub, content.Name);
			public INode Create(BehaviorTreeNode_MessageReceived content) => new SubscribeToMessageNode(MessageHub, content.Name);
			public INode Create(BehaviorTreeNode_TargetMessageSender content) => new TargetMessageSenderNode();
			public INode Create(BehaviorTreeNode_SaveTarget content) => new SaveTargetNode(StrinToId(content.Name));
			public INode Create(BehaviorTreeNode_LoadTarget content) => new LoadTargetNode(StrinToId(content.Name));
			public INode Create(BehaviorTreeNode_ForgetMainTarget content) => new ForgetTargetNode();
			public INode Create(BehaviorTreeNode_HasSavedTarget content) => new HasTargetNode(StrinToId(content.Name));
			public INode Create(BehaviorTreeNode_ForgetSavedTarget content) => new ForgetTargetNode(StrinToId(content.Name));
			public INode Create(BehaviorTreeNode_EscapeTargetAttackRadius content) => new EscapeAttackRadiusNode();
			public INode Create(BehaviorTreeNode_HasAdditionalTargets content) => new HasSecondaryTargetsNode();
			public INode Create(BehaviorTreeNode_AttackMainTarget content) => new AttackMainTargetNode(Settings.AiLevel);
			public INode Create(BehaviorTreeNode_AttackAdditionalTargets content) => new AttackSecondaryTargetsNode(Settings.AiLevel);
            public INode Create(BehaviorTreeNode_HasMothership content) => new MothershipAlive();
            public INode Create(BehaviorTreeNode_TargetAllyStarbase content) => new TargetStarbaseNode(true);
            public INode Create(BehaviorTreeNode_TargetEnemyStarbase content) => new TargetStarbaseNode(false);
            public INode Create(BehaviorTreeNode_MakeTargetMothership content) => new SetMothershipNode();
            public INode Create(BehaviorTreeNode_TargetDistance content) => new DistanceToTargetNode(content.MaxDistance > 0 ? content.MaxDistance : Settings.LimitedRange);
            public INode Create(BehaviorTreeNode_HasLongerAttackRange content) => new HasBetterAttackRange(content.Multiplier);
        }
	}
}
