using Combat.Component.Ship;
using Combat.Unit;
using Combat.Component.Controller;
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
        private BallLightningController _ballLightning;
        private StrategicWeaponController _strategicProjectile;
        private Sprite _projectileIcon;
        private Texture2D _projectileTexture;
        private readonly Text[] _resourceValues = new Text[3];

        private void Awake()
        {
            ConfigureContinuousBars();
        }

        public void Close()
        {
            _ballLightning = null;
            _strategicProjectile = null;
            SetResourceValuesVisible(false);
            ReleaseProjectileIcon();
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }

        public void Open(IShip ship)
        {
            if (!ship.IsActive())
                return;

            GetComponent<AnimatedWindow>().Open();
            ConfigureContinuousBars();

            _ballLightning = null;
            _strategicProjectile = null;
            ReleaseProjectileIcon();
            if (_icon)
                _icon.color = Color.white;

            if (_ship == ship)
            {
                EnsureResourceValues();
                SetResourceValuesVisible(true);
                return;
            }

            _ship = ship;

            EnsureResourceValues();
            SetResourceValuesVisible(true);

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

        public void OpenBallLightning(BallLightningController controller)
        {
            if (controller == null || !controller.IsActive)
                return;

            if (_ballLightning == controller)
            {
                UpdateBallLightningIcon();
                return;
            }

            GetComponent<AnimatedWindow>().Open();
            _ship = null;
            _ballLightning = controller;
            _strategicProjectile = null;
            _armorPoints.gameObject.SetActive(false);
            _shieldPoints.gameObject.SetActive(false);
            _energyPoints.gameObject.SetActive(false);
            SetResourceValuesVisible(false);
            HideResistanceRows();
            UpdateBallLightningIcon();
        }

        public void OpenStrategicProjectile(StrategicWeaponController controller)
        {
            if (controller == null || !controller.IsActive ||
                controller.Kind != StrategicWeaponController.WeaponKind.DualVectorFoil)
                return;

            GetComponent<AnimatedWindow>().Open();
            _ship = null;
            _ballLightning = null;
            _strategicProjectile = controller;
            _armorPoints.gameObject.SetActive(false);
            _shieldPoints.gameObject.SetActive(false);
            _energyPoints.gameObject.SetActive(false);
            SetResourceValuesVisible(false);
            HideResistanceRows();
            UpdateStrategicProjectileIcon();
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
            if (_ballLightning != null)
            {
                if (!_ballLightning.IsActive)
                {
                    Close();
                    return;
                }

                UpdateBallLightningIcon();
                return;
            }

            if (_strategicProjectile != null)
            {
                if (!_strategicProjectile.IsActive)
                {
                    Close();
                    return;
                }
                UpdateStrategicProjectileIcon();
                return;
            }

            if (_ship == null)
                return;

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

            UpdateResourceValues();
        }

        private void EnsureResourceValues()
        {
            if (_resourceValues[0] != null || _icon == null)
                return;

            var isPlayerPanel = gameObject.name.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0;
            var iconRect = _icon.rectTransform;
            var horizontalOffset = iconRect.rect.width * 0.5f + 8f;
            var colors = new[]
            {
                new Color(0.35f, 1f, 0.35f, 1f),
                new Color(0.3f, 0.7f, 1f, 1f),
                new Color(1f, 0.9f, 0.2f, 1f)
            };

            for (var i = 0; i < _resourceValues.Length; i++)
            {
                var valueObject = new GameObject("ResourceValue" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                var rect = valueObject.GetComponent<RectTransform>();
                rect.SetParent(iconRect, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(280f, 25f);
                rect.pivot = new Vector2(isPlayerPanel ? 0f : 1f, 0.5f);
                rect.anchoredPosition = new Vector2(isPlayerPanel ? horizontalOffset : -horizontalOffset, 25f - i * 25f);

                var text = valueObject.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 19;
                text.fontStyle = FontStyle.Bold;
                text.alignment = isPlayerPanel ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
                text.color = colors[i];
                text.raycastTarget = false;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                _resourceValues[i] = text;
            }
        }

        private void ConfigureContinuousBars()
        {
            ConfigureContinuousBar(_armorPoints, new Color(0.35f, 1f, 0.35f, 0.96f));
            ConfigureContinuousBar(_shieldPoints, new Color(0.30f, 0.70f, 1f, 0.96f));
            ConfigureContinuousBar(_energyPoints, new Color(1f, 0.90f, 0.20f, 0.96f));
        }

        private static void ConfigureContinuousBar(ProgressBar bar, Color color)
        {
            if (bar == null)
                return;

            bar.UseSolidTexture();
            bar.color = color;
            bar.material = null;
            bar.raycastTarget = false;
            bar.SetAllDirty();
        }

        private void UpdateResourceValues()
        {
            if (_ship == null || _resourceValues[0] == null)
                return;

            _resourceValues[0].text = FormatResource(_ship.Stats.Armor.Value, _ship.Stats.Armor.MaxValue);
            _resourceValues[1].text = FormatResource(_ship.Stats.Shield.Value, _ship.Stats.Shield.MaxValue);
            _resourceValues[2].text = FormatResource(_ship.Stats.Energy.Value, _ship.Stats.Energy.MaxValue);
        }

        private void SetResourceValuesVisible(bool visible)
        {
            foreach (var value in _resourceValues)
                if (value != null)
                    value.gameObject.SetActive(visible);
        }

        private static string FormatResource(float value, float maximum)
        {
            return RoundResourceToInt64(value).ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
                   RoundResourceToInt64(maximum).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static long RoundResourceToInt64(float value)
        {
            if (float.IsNaN(value) || value <= 0f)
                return 0L;
            if (float.IsPositiveInfinity(value) || value >= long.MaxValue)
                return long.MaxValue;

            return (long)System.Math.Round((double)value, System.MidpointRounding.AwayFromZero);
        }

        private void UpdateBallLightningIcon()
        {
            if (_icon == null || _ballLightning == null)
                return;

            var texture = Resources.Load<Texture2D>("Textures/BallLightning/" + _ballLightning.DisplayTextureName);
            if (texture != null && texture != _projectileTexture)
            {
                ReleaseProjectileIcon();
                _projectileTexture = texture;
                _projectileIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 512f);
            }

            _icon.sprite = _projectileIcon;
            _icon.color = _ballLightning.DisplayColor;
        }

        private void UpdateStrategicProjectileIcon()
        {
            if (_icon == null || _strategicProjectile == null)
                return;
            ReleaseProjectileIcon();
            _icon.sprite = _resourceLocator.GetSprite("dual_vector_foil_projectile") ??
                           _resourceLocator.GetSprite("dual_vector_foil_launcher");
            _icon.color = Color.white;
        }

        private void HideResistanceRows()
        {
            _fireResistIcon?.SetActive(false);
            _energyResistIcon?.SetActive(false);
            _kineticResistIcon?.SetActive(false);
            _corrosiveResistIcon?.SetActive(false);
            _fireResistText?.gameObject.SetActive(false);
            _energyResistText?.gameObject.SetActive(false);
            _kineticResistText?.gameObject.SetActive(false);
            _corrosiveResistText?.gameObject.SetActive(false);
        }

        private void ReleaseProjectileIcon()
        {
            if (_projectileIcon != null)
                Destroy(_projectileIcon);
            _projectileIcon = null;
            _projectileTexture = null;
        }

        private void OnDestroy()
        {
            ReleaseProjectileIcon();
        }

        private float _updateResistanceCooldown;
        private bool _hasShield;
        private bool _hasArmor;
        private IShip _ship;
    }
}
