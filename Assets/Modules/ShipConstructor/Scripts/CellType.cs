using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor
{
	public static class CellTypeExtension
	{
		public static bool CompatibleWith(this CellType component, CellType target)
		{
			if (target == CellType.Empty || target == Layout.CustomizableCell)
				return false;
			if (component == CellType.Empty || component == target)
				return true;
			
			if (target == CellType.InnerOuter && (component == CellType.Inner || component == CellType.Outer))
				return true;
            if (component == CellType.InnerOuter && (target == CellType.Inner || target == CellType.Outer))
                return true;

            if (target == CellType.OuterEngine && (component == CellType.Engine || component == CellType.Outer))
                return true;
            if (component == CellType.OuterEngine && (target == CellType.Engine || target == CellType.Outer))
                return true;

            if (target == CellType.OuterEngine && (component == CellType.Engine || component == CellType.Outer))
                return true;
            if (component == CellType.OuterEngine && (target == CellType.Engine || target == CellType.Outer))
                return true;

            if (target == CellType.InnerEngine && (component == CellType.Engine || component == CellType.Inner))
                return true;
            if (component == CellType.InnerEngine && (target == CellType.Engine || target == CellType.Inner))
                return true;

            if (target == CellType.WeaponEngine && (component == CellType.Engine || component == CellType.Weapon))
                return true;
            if (component == CellType.WeaponEngine && (target == CellType.Engine || target == CellType.Weapon))
                return true;

            if (target == CellType.WeaponInner && (component == CellType.Inner || component == CellType.Weapon))
                return true;
            if (component == CellType.WeaponInner && (target == CellType.Inner || target == CellType.Weapon))
                return true;

            if (target == CellType.WeaponOuter && (component == CellType.Outer || component == CellType.Weapon))
                return true;
            if (component == CellType.WeaponOuter && (target == CellType.Outer || target == CellType.Weapon))
                return true;

            if (target == CellType.All)
                return true;

            return false;
		}
	}
}
