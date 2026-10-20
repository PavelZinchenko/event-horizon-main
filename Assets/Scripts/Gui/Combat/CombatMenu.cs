using Combat.Integration;
using Services.Gui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    public class CombatMenu : MonoBehaviour
    {
        [SerializeField] private Button _nextEnemyButton;
        [SerializeField] private Button _changeShipButton;
        [SerializeField] private Button _killAllButton;

        [Inject]
        private void Initialize(IBattleView battle, ICombatCommands commands)
        {
            _battle = battle;
            _commands = commands;
        }

        public void Open()
        {
            GetComponent<IWindow>().Open();
        }

        public void InitializeWindow()
        {
            _nextEnemyButton.gameObject.SetActive(_battle.CanCallReinforcement);
            _nextEnemyButton.interactable = true;
            _changeShipButton.gameObject.SetActive(_battle.CanChangeShip);
#if !UNITY_EDITOR
            _killAllButton.gameObject.SetActive(_battle.KillAllAllowedByRules);
#endif
        }

        public void ExitButtonClicked()
        {
            _commands.Surrender();
        }

        public void NextEnemyButtonClicled()
        {
            _commands.CallReinforcement();
        }

        public void ChangeShipButtonClicked()
        {
            _commands.Retreat(_battle.ControlledShip);
        }

        public void KillThemAll()
        {
            _commands.KillAllEnemies();
        }

        // The window object is inactive while closed, so this runs only while the menu is open.
        private void Update()
        {
            if (_nextEnemyButton.gameObject.activeSelf)
                _nextEnemyButton.interactable = _battle.CanCallReinforcement;
        }

        private IBattleView _battle;
        private ICombatCommands _commands;
    }
}
