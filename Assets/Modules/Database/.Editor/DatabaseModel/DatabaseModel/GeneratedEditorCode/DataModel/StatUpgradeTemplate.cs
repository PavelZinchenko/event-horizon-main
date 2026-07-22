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
	public partial class StatUpgradeTemplate
	{
		partial void OnDataDeserialized(StatUpgradeTemplateSerializable serializable, Database database);
		partial void OnDataSerialized(ref StatUpgradeTemplateSerializable serializable);

		public static StatUpgradeTemplate Create(StatUpgradeTemplateSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new StatUpgradeTemplate(serializable, database);
		}

		private StatUpgradeTemplate(StatUpgradeTemplateSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<StatUpgradeTemplate>(serializable.Id, serializable.FileName);
				MaxLevel = new NumericValue<int>(serializable.MaxLevel, 0, 2147483647);
				Stars = serializable.Stars;
				Credits = serializable.Credits;
				Resources = serializable.Resources;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(StatUpgradeTemplateSerializable serializable)
		{
			serializable.MaxLevel = MaxLevel.Value;
			serializable.Stars = Stars;
			serializable.Credits = Credits;
			serializable.Resources = Resources;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<StatUpgradeTemplate> Id;

		public NumericValue<int> MaxLevel = new NumericValue<int>(0, 0, 2147483647);
		public string Stars;
		public string Credits;
		public string Resources;

		public static StatUpgradeTemplate DefaultValue { get; private set; }
	}
}
