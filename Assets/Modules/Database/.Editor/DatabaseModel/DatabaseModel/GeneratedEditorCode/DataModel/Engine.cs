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
	public partial class Engine
	{
		partial void OnDataDeserialized(EngineSerializable serializable, Database database);
		partial void OnDataSerialized(ref EngineSerializable serializable);

		public static Engine Create(EngineSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Engine(serializable, database);
		}

		public Engine() {}

		private Engine(EngineSerializable serializable, Database database)
		{
			Position = serializable.Position;
			Size = new NumericValue<float>(serializable.Size, 0f, 1f);
			OnDataDeserialized(serializable, database);
		}

		public EngineSerializable Serialize()
		{
			var serializable = new EngineSerializable();
			serializable.Position = Position;
			serializable.Size = Size.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public Vector2 Position;
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 1f);

		public static Engine DefaultValue { get; private set; }
	}
}
