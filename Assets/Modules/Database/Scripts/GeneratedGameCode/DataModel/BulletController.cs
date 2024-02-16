


//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public abstract partial class BulletController 
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

		public static BulletController Create(BulletControllerSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case BulletControllerType.Projectile:
					return new BulletController_Projectile(serializable, loader);
				case BulletControllerType.Homing:
					return new BulletController_Homing(serializable, loader);
				case BulletControllerType.Beam:
					return new BulletController_Beam(serializable, loader);
				default:
                    throw new DatabaseException("BulletController: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(IBulletControllerFactory<T> factory);

		protected BulletController(BulletControllerSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;

			OnDataDeserialized(serializable, loader);
		}


		public BulletControllerType Type { get; private set; }

		public static BulletController DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface IBulletControllerFactory<T>
    {
	    T Create(BulletController_Projectile content);
	    T Create(BulletController_Homing content);
	    T Create(BulletController_Beam content);
    }

    public partial class BulletController_Projectile : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Projectile(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }



    }
    public partial class BulletController_Homing : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Homing(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			StartingVelocityModifier = UnityEngine.Mathf.Clamp(serializable.StartingVelocityModifier, 0f, 1000f);
			IgnoreRotation = serializable.IgnoreRotation;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float StartingVelocityModifier { get; private set; }
		public bool IgnoreRotation { get; private set; }


    }
    public partial class BulletController_Beam : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Beam(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }



    }

}

