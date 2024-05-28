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
        private readonly ShipTargetLocator _targetLocator;

		public BehaviorTreeBuilder(IScene scene, ILocalization localization)
		{
			_scene = scene;
			_localization = localization;
			_messageHub = new MessageHub();
            _targetLocator = new ShipTargetLocator(scene);
		}

		public ShipBehaviorTree Build(IShip ship, BehaviorTreeModel model, AiSettings settings)
		{
            var capabilities = new ShipCapabilities(ship, settings.AiLevel);
			var builder = new NodeBuilder(ship, capabilities, settings, _messageHub, _targetLocator, _localization);
            var context = new Context(ship, capabilities, _scene);
			return new ShipBehaviorTree(ship, builder.Build(model.RootNode), context);
		}
	}

	public readonly struct AiSettings
	{
		public readonly float LimitedRange;
		public readonly AiDifficultyLevel AiLevel;

        public static AiSettings ForDrone(float droneRange) => new(AiDifficultyLevel.Hard, droneRange);
        public static AiSettings ForMercenary(float range, int aiLevel) => new(AiLevelFromValue(aiLevel), range);
        public static AiSettings FromAiLevel(int aiLevel) => new(AiLevelFromValue(aiLevel));
		public static AiSettings FromAiLevel(AiDifficultyLevel aiLevel) => new(aiLevel);
		public static AiSettings Default => new(AiDifficultyLevel.Hard);

		private AiSettings(AiDifficultyLevel aiLevel, float droneRange = 0f)
		{
			AiLevel = aiLevel;
			LimitedRange = droneRange;
		}

		private static AiDifficultyLevel AiLevelFromValue(int value)
		{
			if (value < 50) return AiDifficultyLevel.Easy;
			if (value < 80) return AiDifficultyLevel.Medium;
			return AiDifficultyLevel.Hard;
		}
	}
}
