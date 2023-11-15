namespace Services.Localization
{
	internal struct PluralForm
	{
		public readonly int Id;
		public readonly int RangeMin;
		public readonly int RangeMax;
		public readonly int Basis;

		public static PluralForm FromString(string format)
		{
			var items = format.Split(':');
			var id = System.Convert.ToInt32(items[1]);
			items = items[0].Split('%');
			var basis = items.Length > 1 ? System.Convert.ToInt32(items[1]) : 0;
			items = items[0].Split('-');

			int min = int.MinValue;
			int max = int.MaxValue;
			if (items[0] != "*")
			{
				min = System.Convert.ToInt32(items[0]);
				max = items.Length > 1 ? System.Convert.ToInt32(items[1]) : min;
			}

			return new PluralForm(id, min, max, basis);
		}

		public bool IsMatch(int value)
		{
			if (Basis > 0)
				value %= Basis;
			return value >= RangeMin && value <= RangeMax;
		}

		private PluralForm(int id, int min, int max, int basis)
		{
			Id = id;
			RangeMin = min;
			RangeMax = max;
			Basis = basis;
		}
	}
}
