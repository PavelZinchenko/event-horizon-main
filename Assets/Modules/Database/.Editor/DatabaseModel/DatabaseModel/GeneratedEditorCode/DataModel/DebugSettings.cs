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
	public partial class DebugSettings
	{
		partial void OnDataDeserialized(DebugSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref DebugSettingsSerializable serializable);

		public static DebugSettings Create(DebugSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new DebugSettings(serializable, database);
		}

		private DebugSettings(DebugSettingsSerializable serializable, Database database)
		{
			Codes = serializable.Codes?.Select(item => DebugCode.Create(item, database)).ToArray();
			EnableDebugConsole = serializable.EnableDebugConsole;
			OnDataDeserialized(serializable, database);
		}

		public void Save(DebugSettingsSerializable serializable)
		{
			if (Codes == null || Codes.Length == 0)
			    serializable.Codes = null;
			else
			    serializable.Codes = Codes.Select(item => item.Serialize()).ToArray();
			serializable.EnableDebugConsole = EnableDebugConsole;
			OnDataSerialized(ref serializable);
		}

		public DebugCode[] Codes;
		public bool EnableDebugConsole;

		public static DebugSettings DefaultValue { get; private set; }
	}
}
