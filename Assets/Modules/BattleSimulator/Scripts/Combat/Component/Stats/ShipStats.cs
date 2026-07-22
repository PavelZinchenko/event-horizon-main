using Combat.Collision;
using Combat.Component.Mods;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.Ship;
using Combat.Unit;
using Combat.Unit.HitPoints;
using Constructor;
using System.Linq;

namespace Combat.Component.Stats
{
    public class ShipStats : IStats
    {
        public ShipStats(IShipSpecification spec, bool collectStatistics)
        {
            var stats = spec.Stats;

            if (collectStatistics)
                _performance = new ShipPerformance();

            _resistance = new Resistance
            {
                Energy = stats.EnergyResistancePercentage,
                EnergyAbsorption = stats.EnergyAbsorptionPercentage,
                Heat = stats.ThermalResistancePercentage,
                Kinetic = stats.KineticResistancePercentage,
                Corrosive = stats.CorrosiveResistancePercentage,
                ShieldCorrosive = stats.ShieldCorrosiveResistancePercentage
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

            if (stats.ShieldPoints > 0)
                _shieldPoints = new Energy(stats.ShieldPoints, stats.ShieldRechargeRate, stats.ShieldRechargeCooldown);
            else
                _shieldPoints = new EmptyResources();

            _energyPoints = new Energy(stats.EnergyPoints, stats.EnergyRechargeRate, stats.EnergyRechargeCooldown);

            var modifications = spec.ThreeBodyModifications;
            _adaptiveArmorCount = UnityEngine.Mathf.Clamp(modifications.AdaptiveArmorCount, 0, 10);
            _energyLeechCount = UnityEngine.Mathf.Clamp(modifications.EnergyLeechCount, 0, 10);
            _shieldRecirculationCount = UnityEngine.Mathf.Clamp(modifications.ShieldRecirculationCount, 0, 5);
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
        public ShipPerformance Performance => _performance;
        public float TimeFromLastHit { get; private set; }

        public void ApplyDamage(Impact impact, IUnit self, IUnit source)
        {
            if (!IsAlive)
				return;

            if (IsFourDimensionalUnit(source))
                impact.ConvertAllDamageToTrue();

            if (IsFourDimensionalUnit(self))
            {
                impact.KineticDamage = 0f;
                impact.EnergyDamage = 0f;
                impact.HeatDamage = 0f;
                impact.CorrosiveDamage = 0f;
                impact.ShieldDamage = 0f;
            }

            var resistance = Resistance;
            
            impact.ApplyShield(Shield.Value, resistance.ShieldCorrosive);

            var resolvedImpact = impact.GetDamage(resistance);
            ApplyAdaptiveArmor(ref resolvedImpact);
            DamageIndicator?.ApplyDamage(resolvedImpact);

            var damage = resolvedImpact.KineticDamage + resolvedImpact.EnergyDamage +
                         resolvedImpact.HeatDamage + resolvedImpact.CorrosiveDamage + resolvedImpact.TrueDamage;
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

            // Refit reactions are deliberately based on actual hull loss. A
            // depleted shield or a direct energy drain must not trigger them.
            if (damageDealt > 0f)
            {
                if (_energyLeechCount > 0 && Armor.MaxValue > 0f && Energy.MaxValue > 0f)
                {
                    var energyFraction = damageDealt / Armor.MaxValue * 0.5f * _energyLeechCount;
                    Energy.Get(-Energy.MaxValue * energyFraction);
                }

                if (_shieldRecirculationCount > 0 && Shield.Exists)
                    Shield.Get(-damageDealt * 0.2f * _shieldRecirculationCount);
            }

            if (impact.Effects.Contains(CollisionEffect.Destroy))
                damageDealt = Armor.Get(Armor.MaxValue);

            UpdateStatistics(self, source, damageDealt, shieldDamage);
        }

        private void UpdateStatistics(IUnit self, IUnit source, float armorDamage, float shieldDamage)
        {
            if (armorDamage > 0)
                _performance?.OnArmorDamageReceived(armorDamage);
            if (shieldDamage > 0)
                _performance?.OnShieldDamageReceived(armorDamage);

            var owner = source.GetOwnerShip();
            if (owner == null) return;
            var isAlly = owner.Type.Side.IsAlly(self.Type.Side);
            var enemyPerformance = owner.Stats.Performance;
            if (enemyPerformance == null) return;

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

        private void ApplyAdaptiveArmor(ref Impact impact)
        {
            if (_adaptiveArmorCount <= 0 || Armor.MaxValue <= 0f)
                return;

            // Each copy gives one percent independent reduction for every
            // missing ten percent of hull.  Ten copies therefore cap at the
            // documented ninety percent shortly before destruction.
            // The small epsilon makes an exact ten-percent loss count as the
            // first documented threshold despite floating point rounding.
            var missingSteps = UnityEngine.Mathf.Clamp(
                UnityEngine.Mathf.FloorToInt((1f - Armor.Percentage) * 10f + 0.0001f), 0, 9);
            if (missingSteps <= 0)
                return;

            var multiplier = UnityEngine.Mathf.Clamp(1f - missingSteps * _adaptiveArmorCount * 0.01f, 0.1f, 1f);
            impact.KineticDamage *= multiplier;
            impact.EnergyDamage *= multiplier;
            impact.HeatDamage *= multiplier;
            impact.CorrosiveDamage *= multiplier;
            // True damage deliberately stays outside the four basic types.
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

        private readonly ShipPerformance _performance;
        private readonly IResourcePoints _armorPoints;
        private readonly IResourcePoints _shieldPoints;
        private readonly IResourcePoints _energyPoints;
        private readonly Resistance _resistance;
        private readonly Modifications<Resistance> _modifications = new Modifications<Resistance>();
        private readonly int _adaptiveArmorCount;
        private readonly int _energyLeechCount;
        private readonly int _shieldRecirculationCount;

        public static bool IsFourDimensionalUnit(IUnit unit)
        {
            var ship = unit.GetOwnerShip();
            if (ship == null)
                return false;

            // Trisolaris vessels are three-dimensional.  Some of their older
            // generated builds were copied from the four-dimensional demo
            // ships and can retain that feature flag, which incorrectly made
            // them immune to black-domain slowing and ordinary damage.  Use
            // the stable ship IDs as the source of truth for this faction.
            var shipId = ship.Specification?.Info.Id.Value ?? -1;
            if (shipId == 166 || (shipId >= 1145140 && shipId <= 1145143))
                return false;

            // Keep the faction-level fallback as well.  Imported Trisolaris
            // variants may receive a different generated ship id, but they
            // still belong to faction 22 and must remain ordinary
            // three-dimensional units for black-domain and damage rules.
            if (ship.Type != null && ship.Type.FactionId == 22)
                return false;

            if (ship.Specification?.Stats?.ShipModel?.Features?.IsFourDimensional == true)
                return true;

            return ship.Systems?.All?.OfType<Combat.Component.Systems.Devices.DimensionalAscensionDevice>().Any(device => device.IsDimensionShifted) == true;
        }
    }
}
