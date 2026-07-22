using GameDatabase.Enums;

namespace Combat.Collision
{
    public struct Resistance
    {
        public float Kinetic;
        public float Energy;
        public float Heat;
        public float Corrosive;
        public float EnergyAbsorption;
        public float ShieldCorrosive;

        public float ModifyKineticDamage(float damage) => ModifyDamage(damage, Kinetic);
        public float ModifyEnergyDamage(float damage) => ModifyDamage(damage, Energy);
		public float ModifyHeatDamage(float damage) => ModifyDamage(damage, Heat);
		public float ModifyCorrosiveDamage(float damage) => ModifyDamage(damage, Corrosive);

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
				case DamageType.Corrosive:
					return ModifyCorrosiveDamage(damage);
				case DamageType.True:
					return damage;
				default:
					return damage;
			}
		}

        public static readonly Resistance Empty = new();
	}
}
