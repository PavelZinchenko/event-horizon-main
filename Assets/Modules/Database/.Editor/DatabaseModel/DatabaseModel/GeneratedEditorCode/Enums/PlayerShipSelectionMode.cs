//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

namespace EditorDatabase.Enums
{
	public enum PlayerShipSelectionMode
	{
		Default = 0,
		[TooltipText("Player can select only one ship. When it dies, battle ends")]
		OnlyOneShip = 1,
		[TooltipText("Ships enters battlefield by order. Selection is not allowed")]
		ByOrder = 2,
		[TooltipText("Player can select ship but can't change it until it dies")]
		NoRetreats = 3,
	}
}
