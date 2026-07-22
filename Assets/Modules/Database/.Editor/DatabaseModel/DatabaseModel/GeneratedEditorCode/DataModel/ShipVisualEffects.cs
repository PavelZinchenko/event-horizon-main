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
	public partial class ShipVisualEffects
	{
		partial void OnDataDeserialized(ShipVisualEffectsSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipVisualEffectsSerializable serializable);

		public static ShipVisualEffects Create(ShipVisualEffectsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipVisualEffects(serializable, database);
		}

		public ShipVisualEffects() {}

		private ShipVisualEffects(ShipVisualEffectsSerializable serializable, Database database)
		{
			LeaveWreck = serializable.LeaveWreck;
			CustomExplosionEffect = database.GetVisualEffectId(serializable.CustomExplosionEffect);
			CustomExplosionSound = serializable.CustomExplosionSound;
			OnDataDeserialized(serializable, database);
		}

		public ShipVisualEffectsSerializable Serialize()
		{
			var serializable = new ShipVisualEffectsSerializable();
			serializable.LeaveWreck = LeaveWreck;
			serializable.CustomExplosionEffect = CustomExplosionEffect.Value;
			serializable.CustomExplosionSound = CustomExplosionSound;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ToggleState LeaveWreck;
		public ItemId<VisualEffect> CustomExplosionEffect = ItemId<VisualEffect>.Empty;
		public string CustomExplosionSound;

		public static ShipVisualEffects DefaultValue { get; private set; }
	}
}
