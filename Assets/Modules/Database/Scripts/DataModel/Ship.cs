using GameDatabase.Model;
using GameDatabase.Serializable;
using GameDatabase.Utils;

namespace GameDatabase.DataModel
{
    public partial class Ship
    {
        partial void OnDataDeserialized(ShipSerializable serializable, Database.Loader loader)
        {
            Barrels = new ImmutableCollection<Barrel>(BarrelConverter.Convert(Layout, serializable.Barrels, serializable.Id));

            for (var i = 0; i < Collider.Count; ++i)
                if (!Collider[i].IsValid)
                    GameDiagnostics.Trace.LogError($"invalid collider shape {i} in ship {serializable.Id}");
        }
    }
}
