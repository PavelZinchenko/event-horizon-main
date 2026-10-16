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
	public partial class StatModification 
	{
		partial void OnDataDeserialized(StatModificationSerializable serializable, Database.Loader loader);

		public static StatModification Create(StatModificationSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new StatModification(serializable, loader);
		}

		private StatModification(StatModificationSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;
			Gray3 = Utilites.MathEx.Clamp(serializable.Gray3, -3.402823E+38f, 3.402823E+38f);
			Gray2 = Utilites.MathEx.Clamp(serializable.Gray2, -3.402823E+38f, 3.402823E+38f);
			Gray1 = Utilites.MathEx.Clamp(serializable.Gray1, -3.402823E+38f, 3.402823E+38f);
			Green = Utilites.MathEx.Clamp(serializable.Green, -3.402823E+38f, 3.402823E+38f);
			Purple = Utilites.MathEx.Clamp(serializable.Purple, -3.402823E+38f, 3.402823E+38f);
			Gold = Utilites.MathEx.Clamp(serializable.Gold, -3.402823E+38f, 3.402823E+38f);

			OnDataDeserialized(serializable, loader);
		}

		public StatModificationType Type { get; private set; }
		public float Gray3 { get; private set; }
		public float Gray2 { get; private set; }
		public float Gray1 { get; private set; }
		public float Green { get; private set; }
		public float Purple { get; private set; }
		public float Gold { get; private set; }

		public static StatModification DefaultValue { get; private set; }= new(new(), null);
	}
}
