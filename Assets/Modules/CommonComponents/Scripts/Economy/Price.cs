using System;

namespace Economy
{
    public struct Price
    {
        public Price(long amount, Currency currency)
        {
            if (amount < 0)
                throw new ArgumentException("negative price: " + amount + " " + currency);

            _amount = amount;
            _currency = currency;
        }

        public static Price Common(long amount)
        {
            return new Price(amount, Currency.Credits);
        }

        public static string PriceToString(long amount)
        {
            if (amount < 10000)
                return amount.ToString();
            if (amount < 10000000)
                return amount/1000 + "K";
            else
                return amount/1000000 + "M";
        }

        public static Price Premium(long amount)
        {
#if IAP_DISABLED
            return new Price(amount*500, Currency.Credits);
#else
            return new Price(amount, Currency.Stars);
#endif
        }

        public static Price Tokens(long amount)
        {
            return new Price(amount, Currency.Tokens);
        }

        public static Price operator *(Price price, int multiplier)
        {
            return new Price(price._amount*multiplier, price._currency);
        }

        public static Price operator *(Price price, float multiplier)
        {
            var value = (long)Math.Ceiling((double) price._amount * multiplier);
            return new Price(value, price._currency);
        }

        public static Price operator /(Price price, int multiplier)
        {
            if (price._amount <= 0)
                return new Price(0, price._currency);

            return new Price(Math.Max(price._amount / multiplier, 1), price._currency);
        }

        public override string ToString()
        {
            return _amount.ToString();
        }

        public int Amount { get { return UnityEngine.Mathf.Max((int)_amount, 0); } }
        public Currency Currency { get { return _currency; } }

        private readonly ObscuredLong _amount;
        private readonly Currency _currency;
    }
}
