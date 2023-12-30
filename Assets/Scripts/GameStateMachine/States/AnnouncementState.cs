using System.Collections.Generic;
using GameServices.SceneManager;
using GameServices.Settings;
using Gui.Common;
using Gui.Dialogs;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
    class AnnouncementState : BaseState
    {
        [Inject]
        public AnnouncementState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IGuiManager guiManager,
            GameSettings gameSettings)
            : base(stateMachine, stateFactory)
        {
            _guiManager = guiManager;
            _gameSettings = gameSettings;
        }

        public override StateType Type => StateType.Dialog;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.MainMenu; } }

        protected override void OnLoad()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            if (_gameSettings.DontAskAgainId < AnnouncementWindow.AnnouncementId)
                _guiManager.OpenWindow(WindowNames.AnnouncementWindow, OnDialogClosed);
            else
#endif
                OnDialogClosed(WindowExitCode.Cancel);
        }

        private void OnDialogClosed(WindowExitCode exitCode)
        {
            LoadState(StateFactory.CreateMainMenuState());
        }

        private readonly IGuiManager _guiManager;
        private readonly GameSettings _gameSettings;

        public class Factory : Factory<AnnouncementState> { }
    }
}
