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
	public abstract partial class BehaviorNodeRequirement
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

		public static BehaviorNodeRequirement Create(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case BehaviorRequirementType.Empty:
					return new BehaviorNodeRequirement_Empty(serializable, loader);
				case BehaviorRequirementType.Any:
					return new BehaviorNodeRequirement_Any(serializable, loader);
				case BehaviorRequirementType.All:
					return new BehaviorNodeRequirement_All(serializable, loader);
				case BehaviorRequirementType.None:
					return new BehaviorNodeRequirement_None(serializable, loader);
				case BehaviorRequirementType.AiLevel:
					return new BehaviorNodeRequirement_AiLevel(serializable, loader);
				case BehaviorRequirementType.MinAiLevel:
					return new BehaviorNodeRequirement_MinAiLevel(serializable, loader);
				case BehaviorRequirementType.SizeClass:
					return new BehaviorNodeRequirement_SizeClass(serializable, loader);
				case BehaviorRequirementType.HasDevice:
					return new BehaviorNodeRequirement_HasDevice(serializable, loader);
				case BehaviorRequirementType.HasDrones:
					return new BehaviorNodeRequirement_HasDrones(serializable, loader);
				case BehaviorRequirementType.HasAnyWeapon:
					return new BehaviorNodeRequirement_HasAnyWeapon(serializable, loader);
				case BehaviorRequirementType.CanRepairAllies:
					return new BehaviorNodeRequirement_CanRepairAllies(serializable, loader);
				case BehaviorRequirementType.HasHighRecoilWeapon:
					return new BehaviorNodeRequirement_HasHighRecoilWeapon(serializable, loader);
				case BehaviorRequirementType.HasChargeableWeapon:
					return new BehaviorNodeRequirement_HasChargeableWeapon(serializable, loader);
				case BehaviorRequirementType.HasRemotelyControlledWeapon:
					return new BehaviorNodeRequirement_HasRemotelyControlledWeapon(serializable, loader);
				case BehaviorRequirementType.IsDrone:
					return new BehaviorNodeRequirement_IsDrone(serializable, loader);
				case BehaviorRequirementType.HasKineticResistance:
					return new BehaviorNodeRequirement_HasKineticResistance(serializable, loader);
				case BehaviorRequirementType.HasHighManeuverability:
					return new BehaviorNodeRequirement_HasHighManeuverability(serializable, loader);
				default:
                    throw new DatabaseException("BehaviorNodeRequirement: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(IBehaviorNodeRequirementFactory<T> factory);

		protected BehaviorNodeRequirement(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;

			OnDataDeserialized(serializable, loader);
		}

		public BehaviorRequirementType Type { get; private set; }

		public static BehaviorNodeRequirement DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface IBehaviorNodeRequirementFactory<T>
    {
	    T Create(BehaviorNodeRequirement_Empty content);
	    T Create(BehaviorNodeRequirement_Any content);
	    T Create(BehaviorNodeRequirement_All content);
	    T Create(BehaviorNodeRequirement_None content);
	    T Create(BehaviorNodeRequirement_AiLevel content);
	    T Create(BehaviorNodeRequirement_MinAiLevel content);
	    T Create(BehaviorNodeRequirement_SizeClass content);
	    T Create(BehaviorNodeRequirement_HasDevice content);
	    T Create(BehaviorNodeRequirement_HasDrones content);
	    T Create(BehaviorNodeRequirement_HasAnyWeapon content);
	    T Create(BehaviorNodeRequirement_CanRepairAllies content);
	    T Create(BehaviorNodeRequirement_HasHighRecoilWeapon content);
	    T Create(BehaviorNodeRequirement_HasChargeableWeapon content);
	    T Create(BehaviorNodeRequirement_HasRemotelyControlledWeapon content);
	    T Create(BehaviorNodeRequirement_IsDrone content);
	    T Create(BehaviorNodeRequirement_HasKineticResistance content);
	    T Create(BehaviorNodeRequirement_HasHighManeuverability content);
    }

    public partial class BehaviorNodeRequirement_Empty : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_Empty(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_Any : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_Any(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Requirements = new ImmutableCollection<BehaviorNodeRequirement>(serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorNodeRequirement> Requirements { get; private set; }
    }
    public partial class BehaviorNodeRequirement_All : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_All(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Requirements = new ImmutableCollection<BehaviorNodeRequirement>(serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorNodeRequirement> Requirements { get; private set; }
    }
    public partial class BehaviorNodeRequirement_None : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_None(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Requirements = new ImmutableCollection<BehaviorNodeRequirement>(serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, loader)));

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public ImmutableCollection<BehaviorNodeRequirement> Requirements { get; private set; }
    }
    public partial class BehaviorNodeRequirement_AiLevel : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_AiLevel(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			DifficultyLevel = serializable.DifficultyLevel;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public AiDifficultyLevel DifficultyLevel { get; private set; }
    }
    public partial class BehaviorNodeRequirement_MinAiLevel : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_MinAiLevel(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			DifficultyLevel = serializable.DifficultyLevel;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public AiDifficultyLevel DifficultyLevel { get; private set; }
    }
    public partial class BehaviorNodeRequirement_SizeClass : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_SizeClass(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			SizeClass = serializable.SizeClass;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public SizeClass SizeClass { get; private set; }
    }
    public partial class BehaviorNodeRequirement_HasDevice : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasDevice(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			DeviceClass = serializable.DeviceClass;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public DeviceClass DeviceClass { get; private set; }
    }
    public partial class BehaviorNodeRequirement_HasDrones : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasDrones(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_HasAnyWeapon : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasAnyWeapon(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_CanRepairAllies : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_CanRepairAllies(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_HasHighRecoilWeapon : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasHighRecoilWeapon(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_HasChargeableWeapon : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasChargeableWeapon(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_HasRemotelyControlledWeapon : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasRemotelyControlledWeapon(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_IsDrone : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_IsDrone(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

    }
    public partial class BehaviorNodeRequirement_HasKineticResistance : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasKineticResistance(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Value = UnityEngine.Mathf.Clamp(serializable.Value, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Value { get; private set; }
    }
    public partial class BehaviorNodeRequirement_HasHighManeuverability : BehaviorNodeRequirement
    {
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database.Loader loader);

  		public BehaviorNodeRequirement_HasHighManeuverability(BehaviorNodeRequirementSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			Value = UnityEngine.Mathf.Clamp(serializable.Value, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBehaviorNodeRequirementFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Value { get; private set; }
    }

}

