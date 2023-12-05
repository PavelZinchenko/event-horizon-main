namespace Combat.Ai
{
	public class WaitAction : IAction
	{
		public void Perform(Context context, ShipControls controls)
		{
			controls.Course = RotationHelpers.Angle(context.Ship.Body.Position.Direction(context.Enemy.Body.Position));
		}
	}
}
