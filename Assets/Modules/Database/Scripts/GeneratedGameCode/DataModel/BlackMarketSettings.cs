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
	public partial class BlackMarketSettings 
	{
		partial void OnDataDeserialized(BlackMarketSettingsSerializable serializable, Database.Loader loader);

		public static BlackMarketSettings Create(BlackMarketSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new BlackMarketSettings(serializable, loader);
		}

		private BlackMarketSettings(BlackMarketSettingsSerializable serializable, Database.Loader loader)
		{
			ExcludedSatellites = new ImmutableCollection<Satellite>(serializable.ExcludedSatellites?.Select(item => loader.GetSatellite(new ItemId<Satellite>(item), true)));

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<Satellite> ExcludedSatellites { get; private set; }

		public static BlackMarketSettings DefaultValue { get; private set; }
	}
}
