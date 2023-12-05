namespace Combat.Ai
{
	public struct Policy
	{
		public Policy(ICondition condition, IAction action, IAction oppositeAction = null)
		{
			this.condition = condition;
			this.action = action;
		    this.oppositeAction = oppositeAction;
		}

	    public void Perform(Context context, ShipControls controls)
	    {
	        if (condition.IsTrue(context))
                action.Perform(context, controls);
            else if (oppositeAction != null)
                oppositeAction.Perform(context, controls);
	    }
		
		private readonly ICondition condition;
	    private readonly IAction action;
	    private readonly IAction oppositeAction;
	}
}