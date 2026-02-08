using UnityEngine;
using UnityEngine.UI;
using Constructor.Ships;
using Economy;
using GameServices.Player;
using Services.Audio;
using Services.Localization;
using Services.Messenger;
using Services.ObjectPool;
using Zenject;
using CommonComponents;
using GameDatabase;

namespace Gui.StarMap
{
    public class MilitaryBase : MonoBehaviour
    {
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly IGameObjectFactory _gameObjectFactory;
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly PlayerSkills _playerSkills;

        [Inject]
        private void Initialize(IMessenger _messenger)
        {
            _messenger.AddListener<Money>(EventType.MoneyValueChanged, value => UpdateResources());
            _messenger.AddListener<Money>(EventType.StarsValueChanged, value => UpdateResources());
        }

        [SerializeField] private LayoutGroup _shipList;
        [SerializeField] private Image _factionIcon;
        [SerializeField] private Text _factionText;
        [SerializeField] private Text _levelText;
        [SerializeField] private AudioClip _buySound;
        [SerializeField] private Text _creditsText;
        [SerializeField] private Text _starsText;
        [SerializeField] private GameObject _starsPanel;

        private float discount => 1f - Mathf.Min(0.25f, _playerSkills.Experience.Level / 648f); // max(PlayerSkills.Experience.Level) = 162

        public void InitializeWindow()
        {
            var faction = _motherShip.CurrentStar.Region.Faction;
            _factionText.text = _localization.GetString("$MilitaryBase", _localization.GetString(faction.Name));
            _factionIcon.color = faction.Color;
            _levelText.text = Level.ToString();

            UpdateContent();
            UpdateResources();
        }

        public void LevelUpButtonClicked(MilitaryBaseShipItem shipItem)
        {
            var ship = shipItem.Ship;
            if (ship.Experience.Level >= Level || ship.Experience >= Maths.Experience.FromLevel(_database.SkillSettings.MaxPlayerShipsLevel))
                return;

            var price = GetLevelUpPrice(ship, discount);
            if (!price.TryWithdraw(_playerResources))
                return;

            ship.Experience = System.Math.Min((long)Maths.Experience.LevelToExp(_database.SkillSettings.MaxPlayerShipsLevel),
                (long)ship.Experience + ship.Experience.NextLevelCost);
            _soundPlayer.Play(_buySound);

            UpdateContent();
        }

        private void UpdateContent()
        {
            _shipList.transform.InitializeElements<MilitaryBaseShipItem, IShip>(_playerFleet.ActiveShipGroup.Ships, UpdateShipItem, _gameObjectFactory);
        }

        private void UpdateShipItem(MilitaryBaseShipItem item, IShip ship)
        {
            item.Initialize(ship, GetLevelUpPrice(ship, discount), Mathf.Min(Level, _database.SkillSettings.MaxPlayerShipsLevel));
        }

        private void UpdateResources()
        {
            _creditsText.text = _playerResources.Money.ToString();
            _starsText.text = _playerResources.Stars.ToString();
#if IAP_DISABLED
            _starsPanel.SetActive(false);
#else
            _starsPanel.SetActive(true);
#endif
        }

        private int Level { get { return Mathf.Max(5, _motherShip.CurrentStar.Level/2); } }

        private static Price GetLevelUpPrice(IShip ship, float discount)
        {
            var size = 1 + Mathf.Max(0, (int)ship.Model.SizeClass);
            var shipLevel = ship.Experience.Level;
            var scaleLevel = shipLevel / 5;
            var scalar = shipLevel < 100 ? 1.15 : 1.5;
            var startingPrice = 10000 + size * 800;
            var price = startingPrice + 125 * size * scalar * scaleLevel * discount;

            return Price.Common(price);
        }
    }
}
