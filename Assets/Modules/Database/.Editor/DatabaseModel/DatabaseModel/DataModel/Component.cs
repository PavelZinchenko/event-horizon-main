using EditorDatabase.Enums;
using EditorDatabase.Serializable;

namespace EditorDatabase.DataModel
{
	public partial class Component
	{
        partial void OnDataDeserialized(ComponentSerializable serializable, Database database)
        {
            if (!string.IsNullOrEmpty(serializable.CellType))
                CellType = (CellType)serializable.CellType.First();
        }

		partial void OnDataSerialized(ref ComponentSerializable serializable)
        {
            serializable.AmmunitionId = Ammunition.IsNull ? AmmunitionObsolete.Value : Ammunition.Value;
            serializable.CellType = CellType != CellType.Empty ? ((char)CellType).ToString() : null;
        }

        public CellType CellType;
	}
}
