using GameDatabase.DataModel;
using CommonComponents.Utils;

namespace Constructor.Satellites
{
    public interface ISatellite
    {
		string Name { get; }
        Satellite Information { get; }
        IItemCollection<IntegratedComponent> Components { get; }

        bool DataChanged { get; set; }
    }
}
