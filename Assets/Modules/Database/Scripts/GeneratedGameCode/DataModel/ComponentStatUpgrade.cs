


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
	public partial class ComponentStatUpgrade 
	{
		partial void OnDataDeserialized(ComponentStatUpgradeSerializable serializable, Database.Loader loader);

		public static ComponentStatUpgrade Create(ComponentStatUpgradeSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ComponentStatUpgrade(serializable, loader);
		}

		private ComponentStatUpgrade(ComponentStatUpgradeSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ComponentStatUpgrade>(serializable.Id);
			loader.AddComponentStatUpgrade(serializable.Id, this);


			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ComponentStatUpgrade> Id;


		public static ComponentStatUpgrade DefaultValue { get; private set; }
	}
}
