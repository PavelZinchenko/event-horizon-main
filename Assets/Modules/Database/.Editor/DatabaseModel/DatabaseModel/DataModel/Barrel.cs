using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class Barrel
	{
        partial void OnDataDeserialized(BarrelSerializable serializable, Database database)
        {
            Position = new Vector2(serializable.Position.y, serializable.Position.x);
        }

		partial void OnDataSerialized(ref BarrelSerializable serializable)
		{
            serializable.Position = new Vector2(Position.y, Position.x);
            serializable.PlatformType = 0;
		}
    }
}
