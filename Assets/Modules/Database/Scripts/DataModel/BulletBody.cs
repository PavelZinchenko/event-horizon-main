using System;
using GameDatabase.Enums;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class BulletBody
    {
        private static readonly BulletBodySerializable DefaultSerializable = new();
        partial void OnDataDeserialized(BulletBodySerializable serializable, Database.Loader loader)
        {
            // Also see Ammunition.OnDataDeserialized
            if (serializable.Type == BulletTypeObsolete.Projectile)
            {
                return;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (serializable.ParentVelocityEffect != DefaultSerializable.ParentVelocityEffect ||
                serializable.AttachedToParent != DefaultSerializable.AttachedToParent || 
                serializable.AiBulletBehavior != DefaultSerializable.AiBulletBehavior)
            {
                throw new DatabaseException("BulletBody.AttachedToParent, BulletBody.ParentVelocityEffect, and BulletBody.AiBulletBehavior fields can not be used when obsolete BulletBody.Type field is used");
            }

            switch (serializable.Type)
            {
                case BulletTypeObsolete.Projectile:
                    AiBulletBehavior = AiBulletBehavior.Projectile;
                    break;
                // Static is just a projectile unaffected by parent velocity
                case BulletTypeObsolete.Static:
                    AiBulletBehavior = AiBulletBehavior.Projectile;
                    ParentVelocityEffect = 0;
                    break;
                // Homing types
                case BulletTypeObsolete.Homing:
                    AiBulletBehavior = AiBulletBehavior.Homing;
                    break;
                case BulletTypeObsolete.Magnetic:
                    AiBulletBehavior = AiBulletBehavior.Homing;
                    break;
                // Beam type
                case BulletTypeObsolete.Continuous:
                    AiBulletBehavior = AiBulletBehavior.Beam;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}