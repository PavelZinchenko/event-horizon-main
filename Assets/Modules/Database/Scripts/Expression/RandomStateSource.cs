using CodeWriter.ExpressionParser;
using Utilites;

namespace GameDatabase.Model
{
    // Random source for the RANDOM and RANDOM_INT expression functions.
    public sealed class RandomStateSource : IRandomSource
    {
        public static readonly RandomStateSource Shared = new(RandomState.FromTickCount());

        private RandomState _state;

        public RandomStateSource(RandomState state) => _state = state;

        public float Range(float minInclusive, float maxInclusive)
        {
            return minInclusive + (maxInclusive - minInclusive) * _state.NextFloat();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (minInclusive == maxExclusive) return minInclusive;
            if (minInclusive < maxExclusive) return minInclusive + (int)(_state.NextUint() % (uint)(maxExclusive - minInclusive));
            return minInclusive - (int)(_state.NextUint() % (uint)(minInclusive - maxExclusive));
        }
    }
}
