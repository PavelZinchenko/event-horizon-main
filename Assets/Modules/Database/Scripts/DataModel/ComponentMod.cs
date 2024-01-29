namespace GameDatabase.DataModel
{
    public partial class ComponentMod
	{
        public static ComponentMod Empty => DefaultValue;

		static ComponentMod()
		{
			DefaultValue = new ComponentMod();
		}

		private ComponentMod() { Description = string.Empty; }
    }
}
