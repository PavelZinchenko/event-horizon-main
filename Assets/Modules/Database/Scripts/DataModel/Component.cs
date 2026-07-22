using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Component
    {
        public static Component Empty = new Component();

        partial void OnDataDeserialized(ComponentSerializable serializable, Database.Loader loader)
        {
            CellType = string.IsNullOrEmpty(serializable.CellType) ? CellType.Empty : (CellType)serializable.CellType.First();

            // Imported and Three-Body missile launchers were created after the
            // original database and several omitted PossibleModifications entirely.
            // Give every missile/rocket/torpedo the same proven weapon-mod pool used
            // by the stock missile launchers, while preserving any explicit custom list.
            if (loader != null && PossibleModifications.Count == 0 && IsMissileWeapon())
            {
                PossibleModifications = new ImmutableCollection<ComponentMod>(
                    MissileModificationIds.Select(id =>
                        loader.GetComponentMod(new ItemId<ComponentMod>(id), true)));
            }
        }

        private bool IsMissileWeapon()
        {
            if (Ammunition != null && Ammunition.Controller != null &&
                Ammunition.Controller.Type == BulletControllerType.Homing)
                return true;

            if (AmmunitionObsolete == null)
                return false;

            return AmmunitionObsolete.Stats.AmmunitionClass switch
            {
                AmmunitionClassObsolete.EmpMissile => true,
                AmmunitionClassObsolete.HomingImmobilizer => true,
                AmmunitionClassObsolete.HomingTorpedo => true,
                AmmunitionClassObsolete.Rocket => true,
                AmmunitionClassObsolete.UnguidedRocket => true,
                AmmunitionClassObsolete.AcidRocket => true,
                AmmunitionClassObsolete.ClusterMissile => true,
                AmmunitionClassObsolete.HomingCarrier => true,
                _ => false,
            };
        }

        private static readonly int[] MissileModificationIds =
        {
            14, // area of effect
            13, // projectile velocity II
            8,  // projectile velocity
            21, // projectile weight
            6,  // cooldown
            16, // damage II
            5,  // damage
            2,  // energy cost
            7,  // range
            1,  // component weight
        };

        private Component() { Id = ItemId<Component>.Empty; }

        public CellType CellType { get; private set; }
    }
}
