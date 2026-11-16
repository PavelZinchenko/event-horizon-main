using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.Serializable;
using GameDatabase.Utils;

namespace GameDatabase.DataModel
{
    public partial class Satellite
    {
        partial void OnDataDeserialized(SatelliteSerializable serializable, Database.Loader loader)
        {
            Barrels = new ImmutableCollection<Barrel>(BarrelConverter.Convert(Layout, serializable.Barrels, serializable.Id));

            if (SizeClass == SizeClass.Undefined)
                GameDiagnostics.Trace.LogWarning($"satellite {serializable.Id} ({Name}): SizeClass Undefined is deprecated, and support for it will be removed");
        }
    }
}
