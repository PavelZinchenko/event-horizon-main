namespace CommonComponents
{
    public static class PcgRandomAlgorithm
    {
        private const ulong _shiftedDefaultIncrement = 721347520444481703ul;
        private const ulong _defaultincrement = 1442695040888963407ul;
        private const ulong _multiplier = 6364136223846793005ul;

        public static uint NextUint(ref ulong state) => NextUint(ref state, _defaultincrement);

        public static uint NextUint(ref ulong state, ulong increment)
        {
            ulong oldState = state;
            state = unchecked(oldState * _multiplier + increment);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rot = (int)(oldState >> 59);
            uint result = (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            return result;
        }

        public static ulong SequenceToIncrement(ulong sequence = _shiftedDefaultIncrement) => (sequence << 1) | 1;
        public static ulong IncrementToSequence(ulong increment = _defaultincrement) => increment >> 1;

        public static ulong InitializeState(ulong seed, ulong increment = _defaultincrement)
        {
            var state = 0ul;
            NextUint(ref state, increment);
            state += seed;
            NextUint(ref state, increment);
            return state;
        }
    }
}
