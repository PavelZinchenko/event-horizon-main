using System.Collections.Generic;
using GameDatabase.Model;

namespace Combat.Integration
{
    // Read-only description of a ship's controllable systems.
    // System IDs are stable for one deployment; a redeployed ship gets a new description.
    public interface IShipControlDescription
    {
        // System IDs grouped by key, in key order. One group per action button.
        IReadOnlyList<IReadOnlyList<int>> KeyBindings { get; }

        // Systems that the drone-bay button controls.
        IReadOnlyList<int> DroneBays { get; }

        SpriteId GetButtonIcon(int system);

        // True when the button of this key group releases as soon as it is pressed.
        bool ReleaseImmediately(int keyGroup);

        // Cooldown fraction of one system, from 0 (ready) to 1.
        float GetCooldown(int system);
    }

    // Player intent for one ship. Input calls it only when its own value changes,
    // so AI and autopilot keep the values that input does not touch.
    public interface IShipCommandSink
    {
        // 0 to 1.
        void SetThrottle(float throttle);

        // Target heading in degrees; null releases the course.
        void SetCourse(float? course);

        void SetSystemState(int system, bool active);
    }

    public interface IShipControlSource
    {
        // Finds the controls of a ship by an entity ID that this battle has published.
        bool TryGetControls(EntityId ship, out IShipControlDescription description, out IShipCommandSink commands);
    }
}
