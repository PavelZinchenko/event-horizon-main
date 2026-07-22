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
	public partial class FactionsSettings
	{
		partial void OnDataDeserialized(FactionsSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref FactionsSettingsSerializable serializable);

		public static FactionsSettings Create(FactionsSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new FactionsSettings(serializable, database);
		}

		private FactionsSettings(FactionsSettingsSerializable serializable, Database database)
		{
			StarbaseInitialDefense = serializable.StarbaseInitialDefense;
			StarbaseMinDefense = new NumericValue<int>(serializable.StarbaseMinDefense, 1, 2147483647);
			DefenseLossPerEnemyDefeated = new NumericValue<int>(serializable.DefenseLossPerEnemyDefeated, 0, 2147483647);
			OnDataDeserialized(serializable, database);
		}

		public void Save(FactionsSettingsSerializable serializable)
		{
			serializable.StarbaseInitialDefense = StarbaseInitialDefense;
			serializable.StarbaseMinDefense = StarbaseMinDefense.Value;
			serializable.DefenseLossPerEnemyDefeated = DefenseLossPerEnemyDefeated.Value;
			OnDataSerialized(ref serializable);
		}

		public string StarbaseInitialDefense;
		public NumericValue<int> StarbaseMinDefense = new NumericValue<int>(0, 1, 2147483647);
		public NumericValue<int> DefenseLossPerEnemyDefeated = new NumericValue<int>(0, 0, 2147483647);

		public static FactionsSettings DefaultValue { get; private set; }
	}
}
