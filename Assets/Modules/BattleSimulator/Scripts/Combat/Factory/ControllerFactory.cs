using Combat.Ai;
using Combat.Scene;
using Constructor;
using GameDatabase;
using GameDatabase.DataModel;
using Combat.Ai.BehaviorTree;

namespace Combat.Factory
{
    public class ControllerFactory
    {
        private readonly IScene _scene;
        private readonly IDatabase _database;
        private readonly BehaviorTreeBuilder _behaviorTreeBuilder;
        private readonly IKeyboard _keyboard;
        private readonly IMouse _mouse;
        private readonly PlayerInputSignal.Trigger _playerInputTrigger;

        public ControllerFactory(
            IScene scene,
            IDatabase database, 
            BehaviorTreeBuilder behaviorTreeBuilder, 
            IKeyboard keyboard, 
            IMouse mouse,
            PlayerInputSignal.Trigger playerInputTrigger)
        {
            _scene = scene;
            _database = database;
            _behaviorTreeBuilder = behaviorTreeBuilder;
            _keyboard = keyboard;
            _mouse = mouse;
            _playerInputTrigger = playerInputTrigger;
        }

        public IControllerFactory CreateKeyboardController() => new KeyboardController.Factory(_keyboard, _mouse, _playerInputTrigger);

        public IControllerFactory CreateDefaultAiController(int aiLevel, BehaviorTreeModel customAi)
        {
            var aiModel = customAi ?? _database.CombatSettings.EnemyAI;
            if (aiModel != null)
                return new BehaviorTreeController.Factory(aiModel, AiSettings.FromAiLevel(aiLevel), _behaviorTreeBuilder);

            return new Computer.Factory(_scene, aiLevel);
        }

        public IControllerFactory CreateCloneController(BehaviorTreeModel customAi)
        {
            var aiModel = customAi ?? _database.CombatSettings.CloneAI;
            if (aiModel != null)
                return new BehaviorTreeController.Factory(aiModel, AiSettings.Default, _behaviorTreeBuilder);

            return new Clone.Factory(_scene);
        }

        public IControllerFactory CreateStarbaseController(BehaviorTreeModel customAi, bool combatMode)
        {
            var aiModel = customAi ?? _database.CombatSettings.StarbaseAI;
            if (aiModel != null)
                return new BehaviorTreeController.Factory(aiModel, AiSettings.Default, _behaviorTreeBuilder);

            return new Starbase.Factory(_scene, combatMode);
        }

        public IControllerFactory CreateDroneController(DroneBehaviour behaviour, float range, bool improvedAi, BehaviorTreeModel behaviorTree)
        {
            if (behaviorTree == null && !improvedAi)
                behaviorTree = behaviour == DroneBehaviour.Aggressive ?
                    _database.CombatSettings.OffensiveDroneAI :
                    _database.CombatSettings.DefensiveDroneAI;

            if (behaviorTree != null)
                return new BehaviorTreeController.Factory(behaviorTree, AiSettings.ForDrone(range), _behaviorTreeBuilder);

            return new Drone.Factory(_scene, range, behaviour, improvedAi);
        }

        public IControllerFactory CreateAutopilotController()
        {
            if (_database.CombatSettings.AutopilotAI != null)
                return new BehaviorTreeController.Factory(_database.CombatSettings.AutopilotAI, AiSettings.Default, _behaviorTreeBuilder);

            return new Computer.Factory(_scene, 100, true);
        }
    }
}
