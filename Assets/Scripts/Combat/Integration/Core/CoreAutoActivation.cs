using CombatSimulation.Core;

namespace Combat.Integration.Core
{
    // Legacy ShipSystems.ShouldActivateSystem for devices with a key below 0 ("Ship mapping (3a)", row 10). Every pilot
    // holds such a device while energy fraction + 0.1 × (key + 1) ≥ 0; the pilot's own bit for it does not count.
    // Legacy adds activation cost ÷ maximum energy while the device is active; until the snapshot has device state
    // (step 3b), "bit held last tick" stands for "active" (owner, 2026-10-29). Inert devices are skipped (D6).
    public sealed class CoreAutoActivation
    {
        public static readonly CoreAutoActivation None = new CoreAutoActivation(new int[0], new float[0], new float[0], 0UL);

        public static CoreAutoActivation Create(CoreShipMapping mapping)
        {
            if (mapping == null)
                return None;

            var count = 0;
            foreach (var device in mapping.Devices)
                if (IsAutomatic(device)) ++count;

            if (count == 0)
                return None;

            var indices = new int[count];
            var thresholds = new float[count];
            var costs = new float[count];
            var mask = 0UL;
            var next = 0;
            for (var i = 0; i < mapping.Devices.Count; ++i)
            {
                var device = mapping.Devices[i];
                if (!IsAutomatic(device)) continue;

                indices[next] = i;
                thresholds[next] = 0.1f * (device.KeyBinding + 1);
                costs[next] = device.ActivationCost;
                mask |= 1UL << i;
                ++next;
            }

            return new CoreAutoActivation(indices, thresholds, costs, mask);
        }

        // Replaces the automatic bits of the pilot's systems.
        public ulong Apply(in ShipState self, ulong systems)
        {
            if (_mask == 0UL)
                return systems;

            systems &= ~_mask;
            var max = self.Energy.Max;
            var fraction = max > 0f ? self.Energy.Value / max : 0f;
            for (var i = 0; i < _indices.Length; ++i)
            {
                var bit = 1UL << _indices[i];
                var left = fraction + _thresholds[i];
                if ((self.Systems & bit) != 0 && max > 0f)
                    left += _costs[i] / max;
                if (left >= 0f)
                    systems |= bit;
            }

            return systems;
        }

        // Pilots leave these bits to Apply.
        public ulong Mask => _mask;

        private CoreAutoActivation(int[] indices, float[] thresholds, float[] costs, ulong mask)
        {
            _indices = indices;
            _thresholds = thresholds;
            _costs = costs;
            _mask = mask;
        }

        private static bool IsAutomatic(in CoreDeviceInfo device) => device.KeyBinding < 0 && !device.IsInert;

        private readonly int[] _indices;
        private readonly float[] _thresholds;
        private readonly float[] _costs;
        private readonly ulong _mask;
    }
}
