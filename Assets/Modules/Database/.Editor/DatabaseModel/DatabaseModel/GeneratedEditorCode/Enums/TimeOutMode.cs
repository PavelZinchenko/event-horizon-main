//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

namespace EditorDatabase.Enums
{
	public enum TimeOutMode
	{
		CallNextEnemy = 0,
		DrainPlayerHp = 1,
		[TooltipText("Calls next enemy if any and resets the timer, draws the battle otherwise")]
		CallNextEnemyOrDraw = 2,
	}
}
