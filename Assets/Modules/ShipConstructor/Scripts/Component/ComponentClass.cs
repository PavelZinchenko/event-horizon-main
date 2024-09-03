using Constructor.Component;
using GameDatabase.Enums;

namespace Constructor
{
	public static class ComponentExtension
	{
	    public static IComponent Create(this GameDatabase.DataModel.Component component, int shipSize)
	    {
	        return new CommonComponent(component, shipSize);
	    }
    }
}
