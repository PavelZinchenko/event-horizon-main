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
	public partial class ComponentGroupTag
	{
		partial void OnDataDeserialized(ComponentGroupTagSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentGroupTagSerializable serializable);

		public static ComponentGroupTag Create(ComponentGroupTagSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ComponentGroupTag(serializable, database);
		}

		private ComponentGroupTag(ComponentGroupTagSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<ComponentGroupTag>(serializable.Id, serializable.FileName);
				MaxInstallableComponents = new NumericValue<int>(serializable.MaxInstallableComponents, 1, 2147483647);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ComponentGroupTagSerializable serializable)
		{
			serializable.MaxInstallableComponents = MaxInstallableComponents.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<ComponentGroupTag> Id;

		public NumericValue<int> MaxInstallableComponents = new NumericValue<int>(0, 1, 2147483647);

		public static ComponentGroupTag DefaultValue { get; private set; }
	}
}
