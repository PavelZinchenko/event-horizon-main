namespace CommonComponents
{
    public class PcgRandom
    {
        private readonly ulong _increment;
        private ulong _state;

        public ulong Sequence => _increment >> 1;
        public ulong State { get => _state; set => _state = value; }

        public PcgRandom() : this(System.Environment.TickCount) {}

        public PcgRandom(int seed)
        {
            _increment = PcgRandomAlgorithm.SequenceToIncrement();
            _state = PcgRandomAlgorithm.InitializeState((ulong)seed, _increment);
        }

        public PcgRandom(int seed, ulong sequence)
        {
            _increment = PcgRandomAlgorithm.SequenceToIncrement(sequence);
            _state = PcgRandomAlgorithm.InitializeState((ulong)seed, _increment);
        }

        public int Next() => (int)(PcgRandomAlgorithm.NextUint(ref _state, _increment) & 0x7fffffff);
        public uint NextUint() => PcgRandomAlgorithm.NextUint(ref _state, _increment);
        public float NextFloat() => RandomStateExtensions.NextFloat(ref _state, _increment);
        public double NextDouble() => RandomStateExtensions.NextDouble(ref _state, _increment);
    }
}
