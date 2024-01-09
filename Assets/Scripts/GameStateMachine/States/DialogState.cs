using System.Collections.Generic;
using GameServices.SceneManager;
using Scripts.GameStateMachine;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
    class DialogState : BaseState
    {
        [Inject]
        public DialogState(
            string windowName,
            WindowArgs windowArgs,
            System.Action<WindowExitCode> onExitAction,
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IGuiManager guiManager)
            : base(stateMachine, stateFactory)
        {
            _windowName = windowName;
            _windowArgs = windowArgs;
            _guiManager = guiManager;
            _onExitAction = onExitAction;
        }

        public override StateType Type { get { return StateType.Dialog; } }

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.StarMap; } }

		protected override void OnLoad()
        {
            _guiManager.OpenWindow(_windowName, _windowArgs, OnDialogClosed);
        }

        protected virtual void OnExit(WindowExitCode exitCode)
        {
            Unload();
        }

        private void OnDialogClosed(WindowExitCode exitCode)
        {
            if (Condition != GameStateCondition.Active)
                throw new BadGameStateException();

			Unload();
			
			var action = _onExitAction;
            _onExitAction = null;
            if (action != null)
                action.Invoke(exitCode);
        }

        private readonly string _windowName;
        private readonly WindowArgs _windowArgs;
        private readonly IGuiManager _guiManager;
        private System.Action<WindowExitCode> _onExitAction;

        public class Factory : Factory<string, WindowArgs, System.Action<WindowExitCode>, DialogState> { }
    }
}
