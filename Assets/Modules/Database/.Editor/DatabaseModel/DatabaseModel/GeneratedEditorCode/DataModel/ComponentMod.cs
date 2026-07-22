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
	public partial class ComponentMod
	{
		partial void OnDataDeserialized(ComponentModSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentModSerializable serializable);

		public static ComponentMod Create(ComponentModSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ComponentMod(serializable, database);
		}

		private ComponentMod(ComponentModSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<ComponentMod>(serializable.Id, serializable.FileName);
				Description = serializable.Description;
				Modifications = serializable.Modifications?.Select(item => StatModification.Create(item, database)).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ComponentModSerializable serializable)
		{
			serializable.Description = Description;
			if (Modifications == null || Modifications.Length == 0)
			    serializable.Modifications = null;
			else
			    serializable.Modifications = Modifications.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<ComponentMod> Id;

		public string Description;
		public StatModification[] Modifications;

		public static ComponentMod DefaultValue { get; private set; }
	}
}
