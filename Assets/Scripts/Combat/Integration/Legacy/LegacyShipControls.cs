using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
using GameDatabase.Model;

namespace Combat.Integration.Legacy
{
    // Controls a legacy ship through the control contracts. Commands write the ship's controls directly.
    public sealed class LegacyShipControls : IShipControlDescription, IShipCommandSink
    {
        public LegacyShipControls(IShip ship)
        {
            _ship = ship;

            // A ship's system list does not change after creation.
            var systems = ship.Systems.All;
            var keyBindings = systems.GetKeyBindings();
            KeyBindings = keyBindings;
            DroneBays = systems.GetDroneBayIndices().ToList();
            _releaseImmediately = keyBindings.Select(group => group.Select(id => systems[id]).ShouReleaseButtonImmediately()).ToList();
        }

        public IReadOnlyList<IReadOnlyList<int>> KeyBindings { get; }
        public IReadOnlyList<int> DroneBays { get; }

        public SpriteId GetButtonIcon(int system) => _ship.Systems.All[system].ControlButtonIcon;
        public bool ReleaseImmediately(int keyGroup) => _releaseImmediately[keyGroup];
        public float GetCooldown(int system) => _ship.Systems.All[system].Cooldown;

        public void SetThrottle(float throttle) => _ship.Controls.Throttle = throttle;
        public void SetCourse(float? course) => _ship.Controls.Course = course;
        public void SetSystemState(int system, bool active) => _ship.Controls.Systems.SetState(system, active);

        private readonly IShip _ship;
        private readonly List<bool> _releaseImmediately;
    }
}
