using System.Collections.Generic;
using GameServices.SceneManager;
using Domain.Quests;
using GameDatabase.Enums;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
	public class QuestState : BaseState
	{
		[Inject]
		public QuestState(
			IStateMachine stateMachine,
            GameStateFactory stateFactory,
			IGuiManager guiManager,
			GameServices.Player.MotherShip motherShip,
			IUserInteraction userInteraction)
			: base(stateMachine, stateFactory)
		{
			_guiManager = guiManager;
			_motherShip = motherShip;
		    _userInteraction = userInteraction;
		}

		public override StateType Type => StateType.Quest;

		protected override void OnLoad()
		{
		    var args = new WindowArgs(_userInteraction);

            switch (_userInteraction.RequiredView)
			{
			case RequiredViewMode.StarSystem:
				_motherShip.ViewMode = GameServices.Player.ViewMode.StarSystem;
				_guiManager.OpenWindow(Gui.Quests.WindowNames.MiniEventDialog, args, OnDialogClosed);
				break;
			case RequiredViewMode.GalaxyMap:
				_motherShip.ViewMode = GameServices.Player.ViewMode.GalaxyMap;
				_guiManager.OpenWindow(Gui.Quests.WindowNames.EventDialog, args, OnDialogClosed);
				break;
			case RequiredViewMode.StarMap:
				_motherShip.ViewMode = GameServices.Player.ViewMode.StarMap;
				_guiManager.OpenWindow(Gui.Quests.WindowNames.EventDialog, args, OnDialogClosed);
				break;
			default:
				_guiManager.OpenWindow(Gui.Quests.WindowNames.EventDialog, args, OnDialogClosed);
				break;
			}
		}

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.StarMap; } }

		private void OnDialogClosed(WindowExitCode exitCode)
		{
			Unload();
		}

		private readonly IGuiManager _guiManager;
		private readonly IUserInteraction _userInteraction;
		private readonly GameServices.Player.MotherShip _motherShip;

		public class Factory : Factory<IUserInteraction, QuestState> { }
	}
}
