using Economy.Products;
using UnityEngine;
using UnityEngine.UI;
using Economy.ItemType;
using Services.Reources;

namespace ViewModel
{
	public class CargoHoldItem : MonoBehaviour
	{
        [SerializeField] Image _icon;
        [SerializeField] Text _nameText;
        [SerializeField] Text _descriptionText;
        [SerializeField] Text _quantityText;

        public IProduct Product { get; private set; }

        public void Initialize(IProduct item, IResourceLocator resourceLocator)
        {
            Product = item;
            
            _icon.sprite = resourceLocator.GetSprite(item.Type.Icon);
            _icon.color = item.Type.Color;

            _nameText.text = item.Type.Name;

            var description = item.Type.Description;
            _descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
            _descriptionText.text = description;

            _descriptionText.color = _nameText.color = item.Type.Quality.ToColor();

            _quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : string.Empty;
        }
	}
}
