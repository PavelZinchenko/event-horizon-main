using Combat.Component.Ship;
using Combat.Unit;
using Gui.Controls;
using Gui.Windows;
using Services.Gui;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;
using ViewModel;
using Zenject;
using System.Collections.Generic;

namespace Gui.Combat
{
    [RequireComponent(typeof(AnimatedWindow))]
    public class ShipStatsPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] private ProgressBar _armorPoints;
        [SerializeField] private ProgressBar _shieldPoints;
        [SerializeField] private ProgressBar _energyPoints;
        [SerializeField] private Image _icon;
        [SerializeField] private SelectShipPanelItemViewModel _shipItem;
        [SerializeField] private GameObject _fireResistIcon;
        [SerializeField] private GameObject _energyResistIcon;
        [SerializeField] private GameObject _kineticResistIcon;
        [SerializeField] private Text _fireResistText;
        [SerializeField] private Text _energyResistText;
        [SerializeField] private Text _kineticResistText;
        private GameObject _corrosiveResistIcon;
        private Text _corrosiveResistText;

        public void Close()
        {
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }

        public void Open(IShip ship)
        {
            if (!ship.IsActive())
                return;

            GetComponent<AnimatedWindow>().Open();

            if (_ship == ship)
                return;

            _ship = ship;

            if (_icon)
                _icon.sprite = _resourceLocator.GetSprite(ship.Specification.Stats.ShipModel.IconImage) ??
                    _resourceLocator.GetSprite(ship.Specification.Stats.ShipModel.ModelImage);

            _shipItem.SetLevel(ship.Specification.Info.Level);
            _shipItem.SetClass(ship.Specification.Info.Class);

            EnsureCorrosiveResistanceRow();
            UpdateResistance();

            _hasShield = _ship.Stats.Shield.Exists;
            _hasArmor = _ship.Stats.Armor.Exists;

            _shieldPoints.gameObject.SetActive(_hasShield);
            _armorPoints.gameObject.SetActive(_hasArmor);
        }

        private void UpdateResistance()
        {
            var resistance = _ship.Stats.Resistance;

            if (_fireResistIcon != null)
            {
                var active = resistance.Heat > 0.01f;
                _fireResistIcon.gameObject.SetActive(active);
                _fireResistText.gameObject.SetActive(active);
                if (active)
                    _fireResistText.text = Mathf.RoundToInt(resistance.Heat * 100) + "%";
            }

            if (_energyResistIcon != null)
            {
                var active = resistance.Energy > 0.01f;
                _energyResistIcon.gameObject.SetActive(active);
                _energyResistText.gameObject.SetActive(active);
                if (active)
                    _energyResistText.text = Mathf.RoundToInt(resistance.Energy * 100) + "%";
            }

            if (_kineticResistIcon != null)
            {
                var active = resistance.Kinetic > 0.01f;
                _kineticResistIcon.gameObject.SetActive(active);
                _kineticResistText.gameObject.SetActive(active);
                if (active)
                    _kineticResistText.text = Mathf.RoundToInt(resistance.Kinetic * 100) + "%";
            }

            if (_corrosiveResistIcon != null && _corrosiveResistText != null)
            {
                _corrosiveResistIcon.gameObject.SetActive(true);
                _corrosiveResistText.gameObject.SetActive(true);
                _corrosiveResistText.text = Mathf.RoundToInt(resistance.Corrosive * 100) + "%";
            }
        }

        private void EnsureCorrosiveResistanceRow()
        {
            if (_corrosiveResistIcon != null || _kineticResistIcon == null || _kineticResistText == null || _energyResistIcon == null)
                return;

            var iconTemplate = _kineticResistIcon.GetComponent<Image>() ?? _energyResistIcon.GetComponent<Image>();
            var textTemplate = _kineticResistText;
            if (iconTemplate == null || textTemplate == null)
                return;

            _corrosiveResistIcon = Instantiate(iconTemplate.gameObject, _kineticResistIcon.transform.parent);
            _corrosiveResistIcon.name = "CorrosiveResistanceIcon";
            _corrosiveResistIcon.GetComponent<Image>().color = new Color(0.45f, 1f, 0.45f, 1f);
            ShiftBelow(_corrosiveResistIcon.transform as RectTransform, _kineticResistIcon.transform as RectTransform, _energyResistIcon.transform as RectTransform);

            // A distinct biohazard glyph prevents corrosion resistance from
            // being mistaken for one of the three vanilla resistance icons.
            var glyphObject = new GameObject("CorrosionGlyph", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var glyphRect = glyphObject.GetComponent<RectTransform>();
            glyphRect.SetParent(_corrosiveResistIcon.transform, false);
            glyphRect.anchorMin = Vector2.zero;
            glyphRect.anchorMax = Vector2.one;
            glyphRect.offsetMin = glyphRect.offsetMax = Vector2.zero;
            var glyph = glyphObject.GetComponent<Text>();
            glyph.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            glyph.fontSize = 22;
            glyph.alignment = TextAnchor.MiddleCenter;
            glyph.color = new Color(0.02f, 0.2f, 0.04f, 1f);
            glyph.text = "☣";
            glyph.raycastTarget = false;

            _corrosiveResistText = Instantiate(textTemplate, _kineticResistText.transform.parent);
            _corrosiveResistText.name = "CorrosiveResistanceText";
            ShiftBelow(_corrosiveResistText.transform as RectTransform, _kineticResistText.transform as RectTransform, _energyResistText.transform as RectTransform);
            _corrosiveResistText.text = "0%";
        }

        private static void ShiftBelow(RectTransform target, RectTransform reference, RectTransform previous)
        {
            if (target == null || reference == null)
                return;

            var offset = previous != null ? reference.anchoredPosition - previous.anchoredPosition : new Vector2(0f, -28f);
            target.anchoredPosition = reference.anchoredPosition + offset;
        }

        private void Update()
        {
            if (!_ship.IsActive())
            {
                Close();
                return;
            }

            _updateResistanceCooldown -= Time.deltaTime;
            if (_updateResistanceCooldown <= 0)
            {
                _updateResistanceCooldown = 0.5f;
                UpdateResistance();
            }

            var total = 0f;
            if (_hasArmor) total += _ship.Stats.Armor.MaxValue;
            if (_hasShield) total += _ship.Stats.Shield.MaxValue;

            var armor = _hasArmor ? _ship.Stats.Armor.Value : 0;
            var shield = _hasShield ? _ship.Stats.Shield.Value : 0;

            if (_hasArmor)
            {
                _armorPoints.Y0 = 0;
                _armorPoints.Y1 = armor / total;
                _armorPoints.SetAllDirty();
            }
            if (_hasShield)
            {
                _shieldPoints.Y0 = armor / total;
                _shieldPoints.Y1 = (armor + shield) / total;
                _shieldPoints.SetAllDirty();
            }

            var energy = _ship.Stats.Energy.Percentage;
            if (!Mathf.Approximately(_energyPoints.Y1, energy))
            {
                _energyPoints.Y1 = energy;
                _energyPoints.SetAllDirty();
            }
        }

        private float _updateResistanceCooldown;
        private bool _hasShield;
        private bool _hasArmor;
        private IShip _ship;
    }
}
