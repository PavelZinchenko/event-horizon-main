﻿using UnityEngine;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MoveToAttackRange : INode
	{
		private readonly float _lerpMinMax;
		private readonly float _multiplier;

		public MoveToAttackRange(float lerpMinMax, float multiplier)
		{
			_lerpMinMax = lerpMinMax;
			_multiplier = multiplier;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			var min = context.SelectedWeapons.RangeMin;
			var max = context.SelectedWeapons.RangeMax;
			float distance = Mathf.Lerp(min, max, _lerpMinMax) * _multiplier;
			
			if (distance <= 0) 
				return NodeState.Failure;

			if (ShipNavigationHandler.KeepDistance(context.Ship, context.TargetShip, 0, distance, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}
	}
}
