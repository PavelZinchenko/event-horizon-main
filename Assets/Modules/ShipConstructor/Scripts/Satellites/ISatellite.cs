using GameDatabase.DataModel;
using CommonComponents.Utils;

namespace Constructor.Satellites
{
    public interface ISatellite
    {
		string Name { get; }
        Satellite BaseModel { get; }

        [System.Obsolete("Use BaseModel. Remove after satellite mount points phase 9.")]
        Satellite Information { get; }

        IItemCollection<IntegratedComponent> Components { get; }

        bool DataChanged { get; set; }
    }
}
