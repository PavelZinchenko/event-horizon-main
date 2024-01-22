namespace Combat.Component.Systems.DroneBays
{
    public interface IDroneBay : ISystem
    {
        bool TryRestoreDrone();
        float Range { get; }
		int DronesInHangar { get; }
		int ActiveDrones { get; }
	}
}
