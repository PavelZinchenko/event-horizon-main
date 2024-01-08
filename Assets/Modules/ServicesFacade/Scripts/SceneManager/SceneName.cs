using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameServices.SceneManager
{
    public enum GameScene
    {
        Undefined,
        Loader,
		CommonGui,
		MainMenu,
		StarMap,
		Battle,
		SkillTree,
		ShipEditor,
		ConfigureControls,
		Combat,
		Exploration,
		Ehopedia,
		Settings,
	}

    public static class GameSceneExtension
    {
        static GameSceneExtension()
        {
            _sceneNames = Enum.GetValues(typeof(GameScene)).Cast<GameScene>().ToDictionary(id => id.ToSceneName());
        }

        public static GameScene ToGameScene(this string sceneName)
        {
            GameScene scene;
            return _sceneNames.TryGetValue(sceneName, out scene) ? scene : GameScene.Undefined;
        }

        public static string ToSceneName(this GameScene scene)
        {
            switch (scene)
            {
                case GameScene.Undefined:
                    return string.Empty;
                case GameScene.Loader:
                    return "LoaderScene";
				case GameScene.CommonGui:
					return "CommonGuiScene";
				case GameScene.MainMenu:
					return "MainMenuScene";
				case GameScene.StarMap:
					return "StarMapScene";
				case GameScene.Battle:
					return "BattleScene";
				case GameScene.SkillTree:
					return "SkillTreeScene";
				case GameScene.ShipEditor:
					return "ShipEditorScene";
				case GameScene.ConfigureControls:
					return "ConfigureControlsScene";
				case GameScene.Combat:
					return "CombatScene";
				case GameScene.Exploration:
					return "ExplorationScene";
				case GameScene.Ehopedia:
					return "EhopediaScene";
				case GameScene.Settings:
					return "SettingsScene";
				default:
                    throw new InvalidEnumArgumentException("scene", (int)scene, scene.GetType());
            }
        }

        private static readonly Dictionary<string, GameScene> _sceneNames;
    }
}