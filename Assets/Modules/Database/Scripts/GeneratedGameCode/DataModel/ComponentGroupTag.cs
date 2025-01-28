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
	public partial class ComponentGroupTag 
	{
		partial void OnDataDeserialized(ComponentGroupTagSerializable serializable, Database.Loader loader);

		public static ComponentGroupTag Create(ComponentGroupTagSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ComponentGroupTag(serializable, loader);
		}

		private ComponentGroupTag(ComponentGroupTagSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ComponentGroupTag>(serializable.Id);
			loader.AddComponentGroupTag(serializable.Id, this);

			MaxInstallableComponents = UnityEngine.Mathf.Clamp(serializable.MaxInstallableComponents, 1, 2147483647);

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ComponentGroupTag> Id;

		public int MaxInstallableComponents { get; private set; }

		public static ComponentGroupTag DefaultValue { get; private set; }
	}
}
