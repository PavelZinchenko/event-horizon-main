using Economy.Products;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;
using Economy.ItemType;

namespace Gui.Exploration
{
    public class LootPanelItem : MonoBehaviour
    {
        [SerializeField] public Image Icon;
        [SerializeField] public Text Name;
        [SerializeField] public Text Description;
        [SerializeField] public Text Quantity;

        public void Initialize(IProduct product, IResourceLocator resourceLocator)
        {
            Icon.gameObject.SetActive(true);
            Icon.sprite = resourceLocator.GetSprite(product.Type.Icon);
            Icon.color = product.Type.Color;
            Name.text = product.Type.Name;
            Name.color = product.Type.Quality.ToColor();

            if (string.IsNullOrEmpty(product.Type.Description))
            {
                Description.gameObject.SetActive(false);
            }
            else
            {
                Description.gameObject.SetActive(true);
                Description.text = product.Type.Description;
            }

            if (product.Quantity <= 1)
            {
                Quantity.gameObject.SetActive(false);
            }
            else
            {
                Quantity.gameObject.SetActive(true);
                Quantity.text = "✕" + product.Quantity;
            }
        }
    }
}
