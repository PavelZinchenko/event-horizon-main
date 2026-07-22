using Combat.Ai.Calculations;
using Combat.Component.Ship.Effects;
using Combat.Component.Unit.Classification;

namespace Combat.Ai.BehaviorTree.Nodes
{
    public class AttackMainTargetNode : INode
    {
        private readonly bool _allowRotation;

        public AttackMainTargetNode(bool allowRotation)
        {
            _allowRotation = allowRotation;
        }

        public NodeState Evaluate(Context context)
        {
            if (RadarStatus.IsJammed(context.Ship))
            {
                ClearTurretTargets(context);
                return NodeState.Failure;
            }

            if (context.TargetShip == null)
                return NodeState.Failure;
            if (!CombatRelations.AreEnemies(context.Ship.Type, context.TargetShip.Type))
                return NodeState.Failure;

            UpdateTargetForTurrets(context);

            AimAndAttackHandler.State result;
            if (_allowRotation)
            {
                result = AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.TargetShip,
                    context.SelectedWeapons, context.Controls);
            }
            else
            {
                result = AimAndAttackHandler.AttackWhileStandingStill(context.Ship,
                    context.TargetShip, context.SelectedWeapons, context.Controls);
            }

            if (HasFlag(result, AimAndAttackHandler.State.Attacking)) return NodeState.Success;
            if (HasFlag(result, AimAndAttackHandler.State.Aiming)) return NodeState.Running;
            return NodeState.Failure;
        }

        private void UpdateTargetForTurrets(Context context)
        {
            if (context.TargetShip == null) return;
            if (!CombatRelations.AreEnemies(context.Ship.Type, context.TargetShip.Type)) return;

            var weapons = context.SelectedWeapons;
            for (int i = 0; i < weapons.List.Count; ++i)
            {
                var weapon = weapons.List[i].Weapon;
                weapon.Platform.ActiveTarget = context.TargetShip;
            }
        }

        private void ClearTurretTargets(Context context)
        {
            var weapons = context.SelectedWeapons;
            for (int i = 0; i < weapons.List.Count; ++i)
                weapons.List[i].Weapon.Platform.ActiveTarget = null;
        }

        private static bool HasFlag(AimAndAttackHandler.State state, AimAndAttackHandler.State flag) => (state & flag) == flag;
    }
}
