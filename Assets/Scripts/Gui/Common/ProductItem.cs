using Economy.Products;
using Economy.ItemType;
using Gui.Common;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.StarMap
{
    public class ProductItem : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;
        [SerializeField] private Text _description;
        [SerializeField] private TextValueItem _quantity;

        public IProduct Product { get; private set; }

        public void Initialize(IResourceLocator resourceLocator, IProduct item)
        {
            Product = item;

            _icon.sprite = resourceLocator.GetSprite(item.Type.Icon);
            _icon.color = item.Type.Color;
            _name.text = item.Type.Name;
            _name.color = Gui.Theme.UiTheme.Current.GetQualityColor(item.Type.Quality);

            if (_description != null)
            {
                _description.text = item.Type.Description;
                _description.color = _name.color;
            }

            if (_quantity != null)
            {
                _quantity.gameObject.SetActive(item.Quantity > 1);
                _quantity.Value = item.Quantity.ToString();
            }
        }
    }
}
