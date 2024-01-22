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
	public partial class DebugSettings
	{
		partial void OnDataDeserialized(DebugSettingsSerializable serializable, Database.Loader loader);

		public static DebugSettings Create(DebugSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new DebugSettings(serializable, loader);
		}

		private DebugSettings(DebugSettingsSerializable serializable, Database.Loader loader)
		{
			Codes = new ImmutableCollection<DebugCode>(serializable.Codes?.Select(item => DebugCode.Create(item, loader)));

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<DebugCode> Codes { get; private set; }

		public static DebugSettings DefaultValue { get; private set; }
	}
}
