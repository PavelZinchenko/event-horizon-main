using System;
using CommonComponents;

namespace Economy
{
    public struct Price
    {
        public Price(Money amount, Currency currency)
        {
            _amount = amount;
            _currency = currency;
        }

        public static Price Common(Money amount)
        {
            return new Price(amount, Currency.Credits);
        }

        public static Price Premium(Money amount)
        {
#if IAP_DISABLED
            return new Price(amount*500, Currency.Credits);
#else
            return new Price(amount, Currency.Stars);
#endif
        }

        public static Price Tokens(Money amount)
        {
            return new Price(amount, Currency.Tokens);
        }

        public static Price operator *(Price price, int multiplier)
        {
            return new Price(price._amount*multiplier, price._currency);
        }

		public static Price operator *(Price price, float multiplier)
		{
			return new Price(price._amount*multiplier, price._currency);
		}

		public Price Min(Money minValue)
		{
			if (_amount < minValue)
				return new Price(minValue, _currency);
			else
				return this;
		}

		public Price Max(Money maxValue)
		{
			if (_amount > maxValue)
				return new Price(maxValue, _currency);
			else
				return this;
		}

		public static Price operator /(Price price, int multiplier)
		{
			if (price._amount <= 0)
				return new Price(0, price._currency);

			return new Price(price._amount / multiplier, price._currency).Min(1);
		}

		public override string ToString()
        {
            return _amount.ToString();
        }

        public Money Amount => _amount;
        public Currency Currency => _currency;

        private readonly Money _amount;
        private readonly Currency _currency;
	}
}
