using Combat.Collision;
using Combat.Component.Mods;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Unit;
using Combat.Unit.HitPoints;
using Constructor;

namespace Combat.Component.Stats
{
    public class ShipStats : IStats
    {
        public ShipStats(IShipSpecification spec)
        {
            var stats = spec.Stats;

            _resistance = new Resistance
            {
                Energy = stats.EnergyResistancePercentage,
                EnergyAbsorption = stats.EnergyAbsorptionPercentage,
                Heat = stats.ThermalResistancePercentage,
                Kinetic = stats.KineticResistancePercentage
            };

            WeaponDamageMultiplier = stats.DamageMultiplier.Value;
            RammingDamageMultiplier = stats.RammingDamageMultiplier;
            HitPointsMultiplier = stats.ArmorMultiplier.Value;

            if (stats.ArmorPoints < 0.1f)
            {
                UnityEngine.Debug.LogError("Creating ship with zero armor - " + spec.Info.Id);
                _armorPoints = new EmptyResources();
            }
            else if (stats.ArmorRepairRate > 0)
                _armorPoints = new Energy(stats.ArmorPoints, stats.ArmorRepairRate, stats.ArmorRepairCooldown);
            else
                _armorPoints = new HitPoints(stats.ArmorPoints);

            _shieldPoints = new Energy(stats.ShieldPoints, stats.ShieldRechargeRate, stats.ShieldRechargeCooldown);
            _energyPoints = new Energy(stats.EnergyPoints, stats.EnergyRechargeRate, stats.EnergyRechargeCooldown);
        }

        public bool IsAlive => _armorPoints.Value > 0;

        public IResourcePoints Armor => _armorPoints;
        public IResourcePoints Shield => _shieldPoints;
        public IResourcePoints Energy => _energyPoints;

        public float WeaponDamageMultiplier { get; private set; }
        public float RammingDamageMultiplier { get; private set; }
        public float HitPointsMultiplier { get; private set; }

        public Resistance Resistance
        {
            get
            {
                var resistance = _resistance;
                _modifications.Apply(ref resistance);
                return resistance;
            }
        }

        public Modifications<Resistance> Modifications => _modifications;
        public IDamageIndicator DamageIndicator { get; set; }
        public ShipPerformance Performance { get; } = new();
        public float TimeFromLastHit { get; private set; }

        public void ApplyDamage(Impact impact, IUnit self, IUnit source)
        {
			if (!IsAlive)
				return;

            if (Shield.Exists)
                impact.ApplyShield(Shield.Value - impact.ShieldDamage);
            else
                impact.ShieldDamage = 0;

            var resistance = Resistance;

            DamageIndicator?.ApplyDamage(impact.GetDamage(resistance));

            var damage = impact.GetTotalDamage(resistance);
            if (damage > 0.1f)
                TimeFromLastHit = 0;

            if (resistance.EnergyAbsorption > 0.01f)
            {
                var energy = resistance.EnergyAbsorption * impact.EnergyDamage/HitPointsMultiplier;
                Energy.Get(-energy);
            }

            damage -= impact.Repair;
            var damageDealt = Armor.Get(damage);
            var energyDamage = Energy.Get(impact.EnergyDrain);
            var shieldDamage = Shield.Get(impact.ShieldDamage);

            if (impact.Effects.Contains(CollisionEffect.Destroy))
                damageDealt = Armor.Get(Armor.MaxValue);

            UpdateStatistics(self, source, damageDealt, shieldDamage);
        }

        private void UpdateStatistics(IUnit self, IUnit source, float armorDamage, float shieldDamage)
        {
            var owner = source.GetOwnerShip();

            if (armorDamage > 0)
                Performance.OnArmorDamageReceived(armorDamage);
            if (shieldDamage > 0)
                Performance.OnShieldDamageReceived(armorDamage);

            if (owner == null) return;
            var isAlly = owner.Type.Side.IsAlly(self.Type.Side);
            var enemyPerformance = owner.Stats.Performance;

            if (isAlly)
            {
                if (armorDamage > 0) enemyPerformance.OnDamageAlly(armorDamage);
                if (shieldDamage > 0) enemyPerformance.OnDamageAlly(shieldDamage);
            }
            else
            {
                if (armorDamage > 0) enemyPerformance.OnDamageArmor(armorDamage);
                if (shieldDamage > 0) enemyPerformance.OnDamageShield(shieldDamage);
            }

            if (armorDamage > 0 && !isAlly && !IsAlive && self.Type.Class == UnitClass.Ship)
                enemyPerformance.OnEnemyKilled();

            if (armorDamage < 0)
                enemyPerformance.OnDamageRepaired(-armorDamage);
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (!IsAlive)
                return;

            DamageIndicator?.Update(elapsedTime);

            _energyPoints.Update(elapsedTime);
            _armorPoints.Update(elapsedTime);
            _shieldPoints.Update(elapsedTime);

            TimeFromLastHit += elapsedTime;
        }

        public void Dispose()
        {
            if (DamageIndicator != null)
                DamageIndicator.Dispose();
        }

        private readonly IResourcePoints _armorPoints;
        private readonly IResourcePoints _shieldPoints;
        private readonly IResourcePoints _energyPoints;
        private readonly Resistance _resistance;
        private readonly Modifications<Resistance> _modifications = new Modifications<Resistance>();
    }
}
