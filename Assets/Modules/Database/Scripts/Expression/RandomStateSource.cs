using CodeWriter.ExpressionParser;
using Utilites;

namespace GameDatabase.Model
{
    // Random source for the RANDOM and RANDOM_INT expression functions.
    public sealed class RandomStateSource : IRandomSource
    {
        public static readonly RandomStateSource Shared = new(RandomState.FromTickCount());

        private RandomState _state;

        [System.ThreadStatic] private static bool _probing;
        [System.ThreadStatic] private static float _probeValue;
        [System.ThreadStatic] private static int _randomCalls;
        [System.ThreadStatic] private static int _randomIntCalls;

        public RandomStateSource(RandomState state) => _state = state;

        // Expression analysis (the combat core mapper samples spawn expressions, phase 5): while evaluate runs, every
        // RANDOM call on this thread returns min + (max - min) x value instead of a draw. The result counts the calls;
        // RANDOM_INT calls return their minimum. Draws outside a probe do not change.
        public static float Probe(System.Func<float> evaluate, float value, out int randomCalls, out int randomIntCalls)
        {
            if (_probing) throw new System.InvalidOperationException("Probes do not nest.");
            _probing = true;
            _probeValue = value;
            _randomCalls = _randomIntCalls = 0;
            try
            {
                return evaluate();
            }
            finally
            {
                _probing = false;
                randomCalls = _randomCalls;
                randomIntCalls = _randomIntCalls;
            }
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            if (_probing)
            {
                ++_randomCalls;
                return minInclusive + (maxInclusive - minInclusive) * _probeValue;
            }
            return minInclusive + (maxInclusive - minInclusive) * _state.NextFloat();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (_probing)
            {
                ++_randomIntCalls;
                return minInclusive;
            }
            if (minInclusive == maxExclusive) return minInclusive;
            if (minInclusive < maxExclusive) return minInclusive + (int)(_state.NextUint() % (uint)(maxExclusive - minInclusive));
            return minInclusive - (int)(_state.NextUint() % (uint)(minInclusive - maxExclusive));
        }
    }
}
