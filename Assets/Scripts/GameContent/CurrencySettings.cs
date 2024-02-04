using UnityEngine;

namespace GameContent
{
    [CreateAssetMenu(fileName = "CurrencySettings", menuName = "ScriptableObjects/CurrencySettings", order = 1)]
    public class CurrencySettings : ScriptableObject
    {
        [SerializeField] private Sprite _creditsIcon;
        [SerializeField] private Color _creditsColor;
        [SerializeField] private Sprite _starsIcon;
        [SerializeField] private Color _starsColor;
        [SerializeField] private Sprite _tokensIcon;
        [SerializeField] private Color _tokensColor;
        [SerializeField] private Sprite _snowflakesIcon;
        [SerializeField] private Color _snowflakesColor;
        [SerializeField] private Sprite _moneyIcon;
        [SerializeField] private Color _moneyColor;

        public Sprite GetIcon(Economy.Currency currency)
        {
            switch (currency) 
            {
                case Economy.Currency.Credits:
                    return _creditsIcon;
                case Economy.Currency.Stars:
                    return _starsIcon;
                case Economy.Currency.Tokens:
                    return _tokensIcon;
                case Economy.Currency.Snowflakes:
                    return _snowflakesIcon;
                case Economy.Currency.Money:
                    return _moneyIcon;
                default:
                    return null;
            }
        }

        [System.Obsolete] public Color GetColor(Economy.Currency currency)
        {
            switch (currency)
            {
                case Economy.Currency.Credits:
                    return _creditsColor;
                case Economy.Currency.Stars:
                    return _starsColor;
                case Economy.Currency.Tokens:
                    return _tokensColor;
                case Economy.Currency.Snowflakes:
                    return _snowflakesColor;
                case Economy.Currency.Money:
                    return _moneyColor;
                default:
                    return Color.white;
            }
        }
    }
}
