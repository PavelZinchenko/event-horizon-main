using System;
using GameDatabase.Enums;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Ammunition
    {
        private static readonly BulletControllerSerializable DefaultController = new();
        partial void OnDataDeserialized(AmmunitionSerializable serializable, Database.Loader loader)
        {
            // Also see BulletBody.OnDataDeserialized
            if (serializable.Body == null || serializable.Body.Type == BulletTypeObsolete.Projectile)
            {
                return;
            }

            if (serializable.Controller != null &&
                (serializable.Controller.Type != DefaultController.Type ||
                 serializable.Controller.IgnoreRotation != DefaultController.IgnoreRotation ||
                 // ReSharper disable once CompareOfFloatsByEqualityOperator
                 serializable.Controller.StartingVelocityModifier != DefaultController.StartingVelocityModifier
                 )
                )
            {
                throw new DatabaseException("Ammunition.Controller field can not be used when obsolete BulletBody.Type field is used in the Ammunition.Body");
            }
            
            switch (serializable.Body.Type)
            {
                // Projectile and static methods don't have special controllers
                case BulletTypeObsolete.Projectile:
                case BulletTypeObsolete.Static:
                    break;
                // Homing controllers
                case BulletTypeObsolete.Homing:
                    Controller = new BulletController_Homing(new BulletControllerSerializable(), loader);
                    break;
                case BulletTypeObsolete.Magnetic:
                    Controller = new BulletController_Homing(new BulletControllerSerializable
                    {
                        IgnoreRotation = true
                    }, loader);
                    break;
                // Beam controller
                case BulletTypeObsolete.Continuous:
                    Controller = new BulletController_Beam(new BulletControllerSerializable(), loader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}