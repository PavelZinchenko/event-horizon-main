using System;
using System.Collections.Generic;
using GameServices.SceneManager;
using Combat.Domain;
using GameModel.Quests;
using GameServices.Player;
using Services.Audio;
using CommonComponents.Signals;
using Zenject;

namespace GameStateMachine.States
{
    public class CombatState : BaseState
    {
        [Inject]
        public CombatState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            ICombatModel combatModel,
            Action<ICombatModel> onCompleteAction,
            PlayerSkills playerSkills,
            MotherShip motherShip,
            IMusicPlayer musicPlayer,
            GameServices.Economy.LootGenerator lootGenerator,
            
            ExitSignal exitSignal,
            CombatCompletedSignal.Trigger combatCompletedTrigger)
            : base(stateMachine, stateFactory)
        {
            _combatModel = combatModel;
            _motherShip = motherShip;
            _lootGenerator = lootGenerator;
            _combatCompletedTrigger = combatCompletedTrigger;
            _playerSkills = playerSkills;
            _musicPlayer = musicPlayer;
            _onCompleteAction = onCompleteAction;

            _exitSignal = exitSignal;
            _exitSignal.Event += OnCombatCompleted;
        }

        public override StateType Type => StateType.Combat;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.Combat; } }

		public override void InstallBindings(DiContainer container)
		{
			container.Bind<ICombatModel>().FromInstance(_combatModel);
		}

		protected override void OnLoad()
        {
            _musicPlayer.Play(AudioTrackType.Combat);
        }

        private void OnCombatCompleted()
        {
			if (Condition != GameStateCondition.Active)
				return;

            var reward = _combatModel.GetReward(_lootGenerator, _playerSkills, _motherShip.CurrentStar);
            reward.Consume(_playerSkills);

            var action = _onCompleteAction;
            _onCompleteAction = null;

            if (action != null)
                action.Invoke(_combatModel);

            _combatCompletedTrigger.Fire(_combatModel);

            if (reward.Any())
                ShowRewardDialog(reward);

			LoadState(StateFactory.CreateStarMapState());
        }

        private void ShowRewardDialog(IReward reward)
        {
            LoadState(StateFactory.CreateCombatRewardState(reward));
        }

        private Action<ICombatModel> _onCompleteAction;
        private readonly ICombatModel _combatModel;
        private readonly ExitSignal _exitSignal;
        private readonly MotherShip _motherShip;
        private readonly GameServices.Economy.LootGenerator _lootGenerator;
        private readonly IMusicPlayer _musicPlayer;
        private readonly CombatCompletedSignal.Trigger _combatCompletedTrigger;
        private readonly PlayerSkills _playerSkills;

        public class Factory : Factory<ICombatModel, Action<ICombatModel>, CombatState> { }
    }

    public class CombatCompletedSignal : SmartWeakSignal<CombatCompletedSignal, ICombatModel> {}
}
