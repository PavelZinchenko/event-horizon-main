//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{

	public interface IBehaviorNodeRequirementContent
	{
		void Load(BehaviorNodeRequirementSerializable serializable, Database database);
		void Save(ref BehaviorNodeRequirementSerializable serializable);
	}

	public partial class BehaviorNodeRequirement : IDataAdapter
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		private static IBehaviorNodeRequirementContent CreateContent(BehaviorRequirementType type)
		{
			switch (type)
			{
				case BehaviorRequirementType.Empty:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.Any:
					return new BehaviorNodeRequirement_Any();
				case BehaviorRequirementType.All:
					return new BehaviorNodeRequirement_All();
				case BehaviorRequirementType.None:
					return new BehaviorNodeRequirement_None();
				case BehaviorRequirementType.AiLevel:
					return new BehaviorNodeRequirement_AiLevel();
				case BehaviorRequirementType.MinAiLevel:
					return new BehaviorNodeRequirement_MinAiLevel();
				case BehaviorRequirementType.SizeClass:
					return new BehaviorNodeRequirement_SizeClass();
				case BehaviorRequirementType.HasDevice:
					return new BehaviorNodeRequirement_HasDevice();
				case BehaviorRequirementType.HasDrones:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasAnyWeapon:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.CanRepairAllies:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasHighRecoilWeapon:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasChargeableWeapon:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasRemotelyControlledWeapon:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasLongRangeWeapon:
					return new BehaviorNodeRequirement_HasLongRangeWeapon();
				case BehaviorRequirementType.HasEngine:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasHarpoon:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.CanRechargeAllies:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.IsDrone:
					return new BehaviorNodeRequirementEmptyContent();
				case BehaviorRequirementType.HasKineticResistance:
					return new BehaviorNodeRequirement_HasKineticResistance();
				case BehaviorRequirementType.HasHighManeuverability:
					return new BehaviorNodeRequirement_HasHighManeuverability();
				case BehaviorRequirementType.HasHighRammingDamage:
					return new BehaviorNodeRequirement_HasHighRammingDamage();
				default:
					throw new DatabaseException("BehaviorNodeRequirement: Invalid content type - " + type);
			}
		}

		public static BehaviorNodeRequirement Create(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BehaviorNodeRequirement(serializable, database);
		}

		public BehaviorNodeRequirement()
		{
			_content = new BehaviorNodeRequirementEmptyContent();
		}

		private BehaviorNodeRequirement(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public BehaviorNodeRequirementSerializable Serialize()
		{
			var serializable = new BehaviorNodeRequirementSerializable();
			serializable.DeviceClass = 0;
			serializable.DifficultyLevel = 0;
			serializable.SizeClass = 0;
			serializable.Value = 1f;
			serializable.Requirements = null;
			_content.Save(ref serializable);
			serializable.Type = Type;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public event System.Action LayoutChangedEvent;
		public event System.Action DataChangedEvent;

		public IEnumerable<IProperty> Properties
		{
			get
			{
				var type = GetType();

				yield return new Property(this, type.GetField("Type"), OnTypeChanged);

				foreach (var item in _content.GetType().GetFields().Where(f => f.IsPublic && !f.IsStatic))
					yield return new Property(_content, item, DataChangedEvent);
			}
		}

		private void OnTypeChanged()
		{
			_content = CreateContent(Type);
			DataChangedEvent?.Invoke();
			LayoutChangedEvent?.Invoke();
		}

		private IBehaviorNodeRequirementContent _content;
		public BehaviorRequirementType Type;

		public static BehaviorNodeRequirement DefaultValue { get; private set; }
	}

	public class BehaviorNodeRequirementEmptyContent : IBehaviorNodeRequirementContent
	{
		public void Load(BehaviorNodeRequirementSerializable serializable, Database database) {}
		public void Save(ref BehaviorNodeRequirementSerializable serializable) {}
	}

	public partial class BehaviorNodeRequirement_Any : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorNodeRequirement[] Requirements;
	}

	public partial class BehaviorNodeRequirement_All : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorNodeRequirement[] Requirements;
	}

	public partial class BehaviorNodeRequirement_None : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Requirements = serializable.Requirements?.Select(item => BehaviorNodeRequirement.Create(item, database)).ToArray();

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			if (Requirements == null || Requirements.Length == 0)
			    serializable.Requirements = null;
			else
			    serializable.Requirements = Requirements.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public BehaviorNodeRequirement[] Requirements;
	}

	public partial class BehaviorNodeRequirement_AiLevel : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			DifficultyLevel = serializable.DifficultyLevel;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.DifficultyLevel = DifficultyLevel;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("AiLevel rises with the level of enemies. Always High for drones and autopilot")]
		public AiDifficultyLevel DifficultyLevel;
	}

	public partial class BehaviorNodeRequirement_MinAiLevel : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			DifficultyLevel = serializable.DifficultyLevel;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.DifficultyLevel = DifficultyLevel;
			OnDataSerialized(ref serializable);
		}

		[TooltipText("AiLevel rises with the level of enemies. Always High for drones and autopilot")]
		public AiDifficultyLevel DifficultyLevel;
	}

	public partial class BehaviorNodeRequirement_SizeClass : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			SizeClass = serializable.SizeClass;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.SizeClass = SizeClass;
			OnDataSerialized(ref serializable);
		}

		public SizeClass SizeClass;
	}

	public partial class BehaviorNodeRequirement_HasDevice : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			DeviceClass = serializable.DeviceClass;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.DeviceClass = DeviceClass;
			OnDataSerialized(ref serializable);
		}

		public DeviceClass DeviceClass;
	}

	public partial class BehaviorNodeRequirement_HasLongRangeWeapon : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Range = new NumericValue<float>(serializable.Value, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.Value = Range.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Range = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorNodeRequirement_HasKineticResistance : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Value = new NumericValue<float>(serializable.Value, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Value = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorNodeRequirement_HasHighManeuverability : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Value = new NumericValue<float>(serializable.Value, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Value = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

	public partial class BehaviorNodeRequirement_HasHighRammingDamage : IBehaviorNodeRequirementContent
	{
		partial void OnDataDeserialized(BehaviorNodeRequirementSerializable serializable, Database database);
		partial void OnDataSerialized(ref BehaviorNodeRequirementSerializable serializable);

		public void Load(BehaviorNodeRequirementSerializable serializable, Database database)
		{
			Value = new NumericValue<float>(serializable.Value, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BehaviorNodeRequirementSerializable serializable)
		{
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Value = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

}

