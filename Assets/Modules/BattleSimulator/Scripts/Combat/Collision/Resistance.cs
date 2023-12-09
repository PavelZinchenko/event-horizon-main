using UnityEngine;
using GameDatabase.Enums;

namespace Combat.Collision
{
    public struct Resistance
    {
        public float Kinetic;
        public float Energy;
        public float Heat;
        public float EnergyDrain;

		public float ModifyKineticDamage(float damage) => damage * (1f - Kinetic);
		public float ModifyEnergyDamage(float damage) => damage * (1f - Energy);
		public float ModifyHeatDamage(float damage) => damage * (1f - Heat);
		public float ModifyDirectDamage(float damage) => damage * (1f - 0.5f * MinResistance);

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

        public static readonly Resistance Empty = new Resistance();
	}
}
