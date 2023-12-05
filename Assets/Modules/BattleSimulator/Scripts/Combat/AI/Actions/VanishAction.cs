namespace Combat.Ai
{
	public class VanishAction : IAction
	{
		public void Perform(Context context, ShipControls controls)
		{
			context.Ship.Vanish();
		}
	}
}
