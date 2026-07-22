//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class ImpactEffect
	{
		partial void OnDataDeserialized(ImpactEffectSerializable serializable, Database database);
		partial void OnDataSerialized(ref ImpactEffectSerializable serializable);

		public static ImpactEffect Create(ImpactEffectSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ImpactEffect(serializable, database);
		}

		public ImpactEffect() {}

		private ImpactEffect(ImpactEffectSerializable serializable, Database database)
		{
			Type = serializable.Type;
			DamageType = serializable.DamageType;
			Power = new NumericValue<float>(serializable.Power, 0f, 1E+09f);
			Factor = new NumericValue<float>(serializable.Factor, 0f, 1E+09f);
			OnDataDeserialized(serializable, database);
		}

		public ImpactEffectSerializable Serialize()
		{
			var serializable = new ImpactEffectSerializable();
			serializable.Type = Type;
			serializable.DamageType = DamageType;
			serializable.Power = Power.Value;
			serializable.Factor = Factor.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ImpactEffectType Type;
		public DamageType DamageType;
		public NumericValue<float> Power = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> Factor = new NumericValue<float>(0, 0f, 1E+09f);

		public static ImpactEffect DefaultValue { get; private set; }
	}
}
