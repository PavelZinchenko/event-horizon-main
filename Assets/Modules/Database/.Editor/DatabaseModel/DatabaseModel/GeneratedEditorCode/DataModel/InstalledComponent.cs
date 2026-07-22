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
	public partial class InstalledComponent
	{
		partial void OnDataDeserialized(InstalledComponentSerializable serializable, Database database);
		partial void OnDataSerialized(ref InstalledComponentSerializable serializable);

		public static InstalledComponent Create(InstalledComponentSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new InstalledComponent(serializable, database);
		}

		public InstalledComponent() {}

		private InstalledComponent(InstalledComponentSerializable serializable, Database database)
		{
			Component = database.GetComponentId(serializable.ComponentId);
			if (Component.IsNull)
			    throw new DatabaseException(this.GetType().Name + ": Component cannot be null");
			Modification = database.GetComponentModId(serializable.Modification);
			Quality = serializable.Quality;
			X = new NumericValue<int>(serializable.X, -32768, 32767);
			Y = new NumericValue<int>(serializable.Y, -32768, 32767);
			BarrelId = new NumericValue<int>(serializable.BarrelId, 0, 255);
			Behaviour = new NumericValue<int>(serializable.Behaviour, 0, 10);
			KeyBinding = new NumericValue<int>(serializable.KeyBinding, -10, 10);
			OnDataDeserialized(serializable, database);
		}

		public InstalledComponentSerializable Serialize()
		{
			var serializable = new InstalledComponentSerializable();
			serializable.ComponentId = Component.Value;
			serializable.Modification = Modification.Value;
			serializable.Quality = Quality;
			serializable.X = X.Value;
			serializable.Y = Y.Value;
			serializable.BarrelId = BarrelId.Value;
			serializable.Behaviour = Behaviour.Value;
			serializable.KeyBinding = KeyBinding.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ItemId<Component> Component = ItemId<Component>.Empty;
		public ItemId<ComponentMod> Modification = ItemId<ComponentMod>.Empty;
		public ModificationQuality Quality;
		public NumericValue<int> X = new NumericValue<int>(0, -32768, 32767);
		public NumericValue<int> Y = new NumericValue<int>(0, -32768, 32767);
		public NumericValue<int> BarrelId = new NumericValue<int>(0, 0, 255);
		public NumericValue<int> Behaviour = new NumericValue<int>(0, 0, 10);
		public NumericValue<int> KeyBinding = new NumericValue<int>(0, -10, 10);

		public static InstalledComponent DefaultValue { get; private set; }
	}
}
