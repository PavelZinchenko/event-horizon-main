using System.Collections.Generic;
using Combat.Domain;
using Combat.Integration.Core;
using GameServices.Audio;
using GameServices.SceneManager;
using Services.Audio;
using Services.GameApplication;
using Zenject;

namespace GameStateMachine.States
{
    // The battle of an online duel (phase 7, step 6; Q2): CoreCombatScene with the duel bound. The state owns the duel:
    // it stops the session thread and leaves the Relay session when it unloads. No reward and no campaign callback.
    // The game never pauses here; the shared battle clock does not stop for one player (owner, 2026-11-14).
    public class OnlineDuelCombatState : BaseState
    {
        [Inject]
        public OnlineDuelCombatState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            CoreDuelBattle duel,
            IApplication application,
            IMusicPlayer musicPlayer,
            DatabaseMusicPlaylist playlist,
            ExitSignal exitSignal)
            : base(stateMachine, stateFactory)
        {
            _duel = duel;
            _application = application;
            _musicPlayer = musicPlayer;
            _playlist = playlist;

            _exitSignal = exitSignal;
            _exitSignal.Event += OnCombatCompleted;
        }

        public override StateType Type => StateType.OnlineDuelCombat;

        public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.CoreCombat; } }

        public override void InstallBindings(DiContainer container)
        {
            container.Bind<ICombatModel>().FromInstance(_duel.Model);
            container.Bind<CoreDuelBattle>().FromInstance(_duel);
        }

        protected override void OnLoad()
        {
            _application.AlwaysRunning = true;
            _playlist.SetCustomCombatPlaylist(_duel.Model.Rules.CustomSoundtrack);
            _musicPlayer.Play(AudioTrackType.Combat);
        }

        protected override void OnUnload()
        {
            _exitSignal.Event -= OnCombatCompleted;
            _application.AlwaysRunning = false;
            _duel.Dispose();
        }

        // Step 7 returns to the lobby with the result.
        private void OnCombatCompleted()
        {
            if (Condition != GameStateCondition.Active)
                return;

            LoadState(StateFactory.CreateMainMenuState());
        }

        private readonly CoreDuelBattle _duel;
        private readonly IApplication _application;
        private readonly ExitSignal _exitSignal;
        private readonly IMusicPlayer _musicPlayer;
        private readonly DatabaseMusicPlaylist _playlist;

        public class Factory : Factory<CoreDuelBattle, OnlineDuelCombatState> { }
    }
}
