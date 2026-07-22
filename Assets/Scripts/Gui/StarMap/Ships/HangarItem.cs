using Constructor.Ships;
using GameDatabase.Enums;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.StarMap
{
    public class HangarItem : MonoBehaviour
    {
        [SerializeField] private Image _allowIcon;
        [SerializeField] private Image _denyIcon;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private Image _emptyImage1;
        [SerializeField] private Image _emptyImage2;
        [SerializeField] private Image _emptyImage3;
        [SerializeField] private Image _emptyImage4;
        [SerializeField] private Image _emptyImage5;

        [Inject] private readonly IResourceLocator _resourceLocator;

        public bool TryInstall(IShip ship)
        {
            if (ship != null && !CanAccept(ship.Model.SizeClass))
                return false;

            Ship = ship;
            return true;
        }

        public bool CanAccept(SizeClass size)
        {
            if (_locked)
                return false;

            return size <= _sizeClass ||
                   (_titanPAllowed && _sizeClass == SizeClass.Titan && size == SizeClass.TitanP);
        }

        /// <summary>
        /// The Giant Cannons skill turns the first two Titan silhouettes into
        /// TitanP-capable hangars.  The property lives on the slot so other
        /// callers (drag, click and restore) share exactly the same rule.
        /// </summary>
        public bool TitanPAllowed
        {
            get { return _titanPAllowed; }
            set
            {
                if (_titanPAllowed == value) return;
                _titanPAllowed = value;
                Initialize();
            }
        }

        public SizeClass Size
        {
            get { return _sizeClass; }
            set
            {
                _sizeClass = value;
                Initialize();
            }
        }

        public Sprite EmptySprite
        {
            get
            {
                switch (_sizeClass)
                {
                    case SizeClass.Frigate:
                        return _emptyImage1.sprite;
                    case SizeClass.Destroyer:
                        return _emptyImage2.sprite;
                    case SizeClass.Cruiser:
                        return _emptyImage3.sprite;
                    case SizeClass.Battleship:
                        return _emptyImage4.sprite;
                    case SizeClass.Titan:
                        return _emptyImage5.sprite;
                    default:
                        return null;
                }
            }
        }

        public void Clear()
        {
            Ship = null;
        }

        public IShip Ship
        {
            get { return _ship; }
            private set
            {
                _ship = value;
                Initialize();
            }
        }

        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
                Initialize();
            }
        }

        public void Highlight(bool enabled, SizeClass requiredClass = SizeClass.Frigate)
        {
            var accepted = CanAccept(requiredClass);
            _allowIcon.gameObject.SetActive(enabled && accepted);
            _denyIcon.gameObject.SetActive(enabled && !accepted);
        }

        private void Initialize()
        {
            if (_locked)
            {
                _icon.gameObject.SetActive(false);
                _emptyImage1.gameObject.SetActive(false);
                _emptyImage2.gameObject.SetActive(false);
                _emptyImage3.gameObject.SetActive(false);
                _emptyImage4.gameObject.SetActive(false);
                _emptyImage5.gameObject.SetActive(false);
                _lockIcon.gameObject.SetActive(true);
                return;
            }

            _lockIcon.gameObject.SetActive(false);

            if (_ship != null)
            {
                _icon.gameObject.SetActive(true);
                _icon.transform.localScale = new Vector3(_ship.Model.SizeClass.IconSize(), _ship.Model.SizeClass.IconSize(), 1.0f);

                _icon.sprite = PlayerShipTextureOverrides.Get(_ship.Model.Id.Value,
                    _resourceLocator.GetSprite(_ship.Model.ModelImage));
                _icon.color = _ship.ColorScheme.HsvColor;
            }
            else
            {
                _icon.gameObject.SetActive(false);
            }

            _emptyImage1.gameObject.SetActive(!_icon.gameObject.activeSelf && _sizeClass == SizeClass.Frigate);
            _emptyImage2.gameObject.SetActive(!_icon.gameObject.activeSelf && _sizeClass == SizeClass.Destroyer);
            _emptyImage3.gameObject.SetActive(!_icon.gameObject.activeSelf && _sizeClass == SizeClass.Cruiser);
            _emptyImage4.gameObject.SetActive(!_icon.gameObject.activeSelf && _sizeClass == SizeClass.Battleship);
            _emptyImage5.gameObject.SetActive(!_icon.gameObject.activeSelf && _sizeClass == SizeClass.Titan);

            // Preserve the original silhouette and add a gold outline only when
            // this Titan hangar has been upgraded to accept TitanP.
            if (_emptyImage5 != null)
            {
                var outline = _emptyImage5.GetComponent<Outline>();
                if (_titanPAllowed && outline == null)
                    outline = _emptyImage5.gameObject.AddComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = new Color(1f, 0.72f, 0.12f, 1f);
                    outline.effectDistance = new Vector2(3f, -3f);
                    outline.enabled = _titanPAllowed && _sizeClass == SizeClass.Titan && !_icon.gameObject.activeSelf;
                }
            }
        }

        private IShip _ship;
        private bool _locked;
        private SizeClass _sizeClass;
        private bool _titanPAllowed;
    }
}
