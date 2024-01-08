using System.Collections.Generic;
using System.Linq;
using GameServices.SceneManager;
using Services.GameApplication;
using Services.Gui;

namespace GameStateMachine.States
{
    public class ModalDialogState : BaseState
    {
		private readonly GameScene[] _gameScenes;
		private readonly string _windowName;
		private readonly IGuiManager _guiManager;
		private readonly IApplication _application;
		private readonly ReloadUiSignal _reloadUiSignal;

        public ModalDialogState(
            IStateMachine stateMachine,
            ReloadUiSignal reloadGuiSignal,
            GameStateFactory stateFactory,
            IApplication application,
            IGuiManager guiManager,
            IEnumerable<GameScene> scenes,
            string windowName)
            : base(stateMachine, stateFactory)
        {
            _gameScenes = scenes.ToArray();
            _windowName = windowName;
            _application = application;
            _guiManager = guiManager;

            _reloadUiSignal = reloadGuiSignal;
            _reloadUiSignal.Event += OnReloadGui;
        }

        public override StateType Type => StateType.ModalDialog;

        public override IEnumerable<GameScene> RequiredScenes => _gameScenes;

        protected override void OnLoad()
        {
			_application.Pause(this);
			UnityEngine.Debug.LogError("OnLoad");
			_guiManager.OpenWindow(_windowName, OnWindowClosed);
        }

        protected override void OnUnload()
        {
			_application.Resume(this);
        }

        private void OnReloadGui()
        {
			Reload();
        }

        private void OnWindowClosed(WindowExitCode exitCode)
        {
			UnityEngine.Debug.LogError("OnWindowClosed");
			Unload();
        }

        public class Factory : Zenject.Factory<string, IEnumerable<GameScene>, ModalDialogState> { }
    }
}
