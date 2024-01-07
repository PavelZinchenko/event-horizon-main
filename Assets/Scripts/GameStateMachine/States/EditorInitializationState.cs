using System.Linq;
using System.Collections.Generic;
using GameServices.SceneManager;
using Constructor.Ships;
using GameDatabase;
using Services.Gui;
using UnityEngine;
using Zenject;

namespace GameStateMachine.States
{
    public class EditorInitializationState : BaseState
    {
        [Inject] private readonly IGuiManager _guiManager;
        [Inject] private readonly IDatabase _database;

        [Inject]
        public EditorInitializationState(IStateMachine stateMachine, GameStateFactory stateFactory)
            : base(stateMachine, stateFactory)
        {
        }

		public override StateType Type => StateType.Initialization;

		protected override void OnLoad()
        {
            string error;
            if (!_database.TryLoad(Application.dataPath + "/../../Database/", out error))
            {
                _guiManager.OpenWindow(global::Gui.Notifications.WindowNames.MessageBoxWindow, new WindowArgs("Database loading failure: " + error), result => Application.Quit());
                return;
            }

            var ship = _database.ShipBuildList.First();
            LoadState(StateFactory.CreateShipEditorState(new ShipEditorState.Context {
				Ship = new EditorModeShip(ship, _database), DatabaseMode = true, NextState = this }));
        }

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.CommonGui; } }

		protected override void OnSuspend()
        {
        }

        protected override void OnResume()
        {
        }

        public class Factory : Factory<EditorInitializationState> { }
    }
}
