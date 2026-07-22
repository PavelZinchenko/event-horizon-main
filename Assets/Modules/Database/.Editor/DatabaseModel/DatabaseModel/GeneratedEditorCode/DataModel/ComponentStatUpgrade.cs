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
	public partial class ComponentStatUpgrade
	{
		partial void OnDataDeserialized(ComponentStatUpgradeSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentStatUpgradeSerializable serializable);

		public static ComponentStatUpgrade Create(ComponentStatUpgradeSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ComponentStatUpgrade(serializable, database);
		}

		private ComponentStatUpgrade(ComponentStatUpgradeSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<ComponentStatUpgrade>(serializable.Id, serializable.FileName);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ComponentStatUpgradeSerializable serializable)
		{
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<ComponentStatUpgrade> Id;


		public static ComponentStatUpgrade DefaultValue { get; private set; }
	}
}
