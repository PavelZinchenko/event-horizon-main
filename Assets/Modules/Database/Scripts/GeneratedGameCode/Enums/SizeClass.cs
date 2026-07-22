//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

namespace GameDatabase.Enums
{
	public enum SizeClass
	{
		Undefined = -1,
		Frigate = 0,
		Destroyer = 1,
		Cruiser = 2,
		Battleship = 3,
		Titan = 4,
		Starbase = 5,
		// Additional capital-ship tier reserved for Titan-class hulls.
		// Keep the explicit value required by the content schema so existing
		// Starbase (5) data remains backward compatible.
		TitanP = 6,
	}
}
