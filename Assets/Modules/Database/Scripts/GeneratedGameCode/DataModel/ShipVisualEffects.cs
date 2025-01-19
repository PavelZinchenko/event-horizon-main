


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
	public partial class ShipVisualEffects 
	{
		partial void OnDataDeserialized(ShipVisualEffectsSerializable serializable, Database.Loader loader);

		public static ShipVisualEffects Create(ShipVisualEffectsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipVisualEffects(serializable, loader);
		}

		private ShipVisualEffects(ShipVisualEffectsSerializable serializable, Database.Loader loader)
		{
			LeaveWreck = serializable.LeaveWreck;
			CustomExplosionEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.CustomExplosionEffect)) ?? VisualEffect.DefaultValue;
			CustomExplosionSound = new AudioClipId(serializable.CustomExplosionSound);

			OnDataDeserialized(serializable, loader);
		}

		public ToggleState LeaveWreck { get; private set; }
		public VisualEffect CustomExplosionEffect { get; private set; }
		public AudioClipId CustomExplosionSound { get; private set; }

		public static ShipVisualEffects DefaultValue { get; private set; }= new(new(), null);
	}
}
