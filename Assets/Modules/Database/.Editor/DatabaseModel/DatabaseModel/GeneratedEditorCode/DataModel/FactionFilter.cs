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
	public partial class RequiredFactions
	{
		partial void OnDataDeserialized(FactionFilterSerializable serializable, Database database);
		partial void OnDataSerialized(ref FactionFilterSerializable serializable);

		public static RequiredFactions Create(FactionFilterSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new RequiredFactions(serializable, database);
		}

		public RequiredFactions() {}

		private RequiredFactions(FactionFilterSerializable serializable, Database database)
		{
			Type = serializable.Type;
			List = serializable.List?.Select(id => new Wrapper<Faction> { Item = database.GetFactionId(id) }).ToArray();
			OnDataDeserialized(serializable, database);
		}

		public FactionFilterSerializable Serialize()
		{
			var serializable = new FactionFilterSerializable();
			serializable.Type = Type;
			if (List == null || List.Length == 0)
			    serializable.List = null;
			else
			    serializable.List = List.Select(wrapper => wrapper.Item.Value).ToArray();
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public FactionFilterType Type;
		public Wrapper<Faction>[] List;

		public static RequiredFactions DefaultValue { get; private set; }
	}
}
