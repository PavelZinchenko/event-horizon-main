using UnityEngine;
using UnityEngine.UI;
using Economy;
using Economy.ItemType;

namespace ViewModel
{
	namespace Common
	{
		public class PricePanel : MonoBehaviour
		{
			[SerializeField] private Text PriceText;
			[SerializeField] private Image CurrencyIcon;
			[SerializeField] private Image BackgroundImage;
			[SerializeField] private Color NotEnoughMoneyColor = new Color(1,0,0,0.75f);
			[SerializeField] private GameContent.CurrencySettings _currencySettings;

		    public void Initialize(Currency currency)
		    {
		        Initialize(null, new Price(0, currency), false);
		        PriceText.text = "???";
		    }

		    public void Initialize(Price price, bool haveMoney = true)
		    {
		        Initialize(null, price, !haveMoney);
		    }

			public void Initialize(IItemType item, Price price, bool notEnoughMoney = false)
			{
			    if (price.Currency == Currency.None)
			    {
			        gameObject.SetActive(false);
			        return;
			    }

                gameObject.SetActive(true);
				PriceText.text = Price.PriceToString(price.Amount);

				var icon = _currencySettings.GetIcon(price.Currency);
				var color = _currencySettings.GetColor(price.Currency);
				CurrencyIcon.sprite = icon;
				CurrencyIcon.color = color;

				if (price.Currency == Currency.Money)
                {
					var iapProduct = item as Services.InAppPurchasing.IIapItem;
					PriceText.text = iapProduct != null ? iapProduct.PriceText : string.Empty;
				}

				if (BackgroundImage != null)
				{
					color.a *= 0.5f;
					BackgroundImage.color = notEnoughMoney ? NotEnoughMoneyColor : color;
				}
			}
		}
	}
}
