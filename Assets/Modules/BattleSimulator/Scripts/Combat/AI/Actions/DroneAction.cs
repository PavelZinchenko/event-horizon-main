namespace Combat.Ai
{
	public class DroneAction : IAction
	{
		public DroneAction(int droneBayId)
		{
			_droneBayId = droneBayId;
		}
		
		public void Perform(Context context, ShipControls controls)
		{
			controls.ActivateSystem(_droneBayId);
		}
		
		private readonly int _droneBayId;
	}
}
