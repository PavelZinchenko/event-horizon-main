using UnityEngine;

namespace Combat.Integration
{
    // A point that the beacon radar shows at the screen edge: an exploration objective (exploration plan, E3). The
    // radar closes when the target goes inactive.
    public interface IBeaconTarget
    {
        Vector2 Position { get; }
        bool IsActive { get; }
    }
}
