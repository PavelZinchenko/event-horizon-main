


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
	public partial class DebugCode 
	{
		partial void OnDataDeserialized(DebugCodeSerializable serializable, Database.Loader loader);

		public static DebugCode Create(DebugCodeSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new DebugCode(serializable, loader);
		}

		private DebugCode(DebugCodeSerializable serializable, Database.Loader loader)
		{
			Code = UnityEngine.Mathf.Clamp(serializable.Code, 0, 999999);
			Loot = LootContent.Create(serializable.Loot, loader);

			OnDataDeserialized(serializable, loader);
		}

		public int Code { get; private set; }
		public LootContent Loot { get; private set; }

		public static DebugCode DefaultValue { get; private set; }= new(new(), null);
	}
}
