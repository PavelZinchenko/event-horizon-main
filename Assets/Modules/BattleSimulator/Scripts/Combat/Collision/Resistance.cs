using GameDatabase.Enums;

namespace Combat.Collision
{
    public struct Resistance
    {
        public float Kinetic;
        public float Energy;
        public float Heat;
        public float EnergyAbsorption;

        public float ModifyKineticDamage(float damage) => ModifyDamage(damage, Kinetic);
        public float ModifyEnergyDamage(float damage) => ModifyDamage(damage, Energy);
		public float ModifyHeatDamage(float damage) => ModifyDamage(damage, Heat);
		public float ModifyDirectDamage(float damage) => ModifyDamage(damage, 0.5f * MinResistance);

        public static float ModifyDamage(float damage, float resistance) => damage > 0 ? damage * (1f - resistance) : damage;

		public float ModifyDamage(DamageType damageType, float damage)
		{
			switch (damageType)
			{
				case DamageType.Impact: 
					return ModifyKineticDamage(damage);
				case DamageType.Energy:
					return ModifyEnergyDamage(damage);
				case DamageType.Heat:
					return ModifyHeatDamage(damage);
				case DamageType.Direct:
					return ModifyDirectDamage(damage);
				default:
					return damage;
			}
		}

		public float MinResistance => Min(Kinetic, Heat, Energy);

		private static float Min(float a, float b, float c)
		{
			if (a < b && a < c) return a;
			return b < c ? b : c;
		}

        public static readonly Resistance Empty = new();
	}
}
