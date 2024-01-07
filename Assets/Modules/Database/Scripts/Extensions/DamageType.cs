using GameDatabase.Enums;

namespace GameDatabase.Extensions 
{
	public static class DamageTypeExtension
	{
		public static string Name(this DamageType type)
		{
			switch (type)
			{
			case DamageType.Impact:
				return "$ImpactDamage";
			case DamageType.Energy:
				return "$EnergyDamage";
			case DamageType.Heat:
				return "$HeatDamage";
			default:
				return "-";
			}
		}
	}
}
