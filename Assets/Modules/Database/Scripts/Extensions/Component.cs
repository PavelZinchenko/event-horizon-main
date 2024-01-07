using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
	public static class ComponentExtensions
	{
		public static ActivationType GetActivationType(this Component component)
		{
			if (component.Weapon != null)
				return component.Weapon.Stats.ActivationType;
			if (component.Device != null)
				return component.Device.Stats.ActivationType;
			if (component.DroneBay != null)
				return component.DroneBay.Stats.ActivationType;

			return ActivationType.None;
		}
	}
}
