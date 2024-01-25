using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;
using Combat.Component.Ship;
using Combat.Scene;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class BehaviorTreeBuilder
	{
		private readonly IScene _scene;
		private readonly ILocalization _localization;
		private readonly MessageHub _messageHub;

		public BehaviorTreeBuilder(IScene scene, ILocalization localization)
		{
			_scene = scene;
			_localization = localization;
			_messageHub = new MessageHub();
		}

		public ShipBehaviorTree Build(IShip ship, BehaviorTreeModel model, AiSettings settings)
		{
			var builder = new NodeBuilder(ship, settings, _messageHub, _localization);
			return new ShipBehaviorTree(ship, _scene, builder.Build(model.RootNode));
		}
	}

	public readonly struct AiSettings
	{
		public readonly float DroneRange;
		public readonly AiDifficultyLevel AiLevel;

		public static AiSettings ForDrone(float droneRange) => new(AiDifficultyLevel.Hard, droneRange);
		public static AiSettings FromAiLevel(int aiLevel) => new(AiLevelFromValue(aiLevel));
		public static AiSettings FromAiLevel(AiDifficultyLevel aiLevel) => new(aiLevel);
		public static AiSettings Default => new(AiDifficultyLevel.Hard);

		private AiSettings(AiDifficultyLevel aiLevel, float droneRange = 0f)
		{
			AiLevel = aiLevel;
			DroneRange = droneRange;
		}

		private static AiDifficultyLevel AiLevelFromValue(int value)
		{
			if (value < 50) return AiDifficultyLevel.Easy;
			if (value < 80) return AiDifficultyLevel.Medium;
			return AiDifficultyLevel.Hard;
		}
	}
}
