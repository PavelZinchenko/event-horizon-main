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
	public partial class LocalizationSettings 
	{
		partial void OnDataDeserialized(LocalizationSettingsSerializable serializable, Database.Loader loader);

		public static LocalizationSettings Create(LocalizationSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new LocalizationSettings(serializable, loader);
		}

		private LocalizationSettings(LocalizationSettingsSerializable serializable, Database.Loader loader)
		{
			CorrosiveDamageText = serializable.CorrosiveDamageText;
			CorrosiveDpsText = serializable.CorrosiveDpsText;

			OnDataDeserialized(serializable, loader);
		}

		public string CorrosiveDamageText { get; private set; }
		public string CorrosiveDpsText { get; private set; }

		public static LocalizationSettings DefaultValue { get; private set; }
	}
}
