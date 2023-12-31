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
            return false;
		}
	}
}
