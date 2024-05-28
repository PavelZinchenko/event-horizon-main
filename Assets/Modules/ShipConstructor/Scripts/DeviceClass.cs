using System.Linq;
using GameDatabase.Enums;

namespace Constructor 
{
	public static class DeviceClassExtension
	{
	    public static bool IsSuitable(this DeviceClass type, Ships.IShipModel ship)
	    {
	        switch (type)
	        {
                case DeviceClass.RepairBot:
	                return !ship.IsBionic;
	        }

            return ship.Stats.BuiltinDevices.All(item => item.Stats.DeviceClass != type);
	    }
	}
}