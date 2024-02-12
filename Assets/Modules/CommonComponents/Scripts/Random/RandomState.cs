namespace CommonComponents
{
    public struct RandomState
    {
        public ulong Value;

        public static RandomState FromSeed(int seed) => new RandomState { Value = PcgRandomAlgorithm.InitializeState((ulong)seed) };
        public static RandomState FromTickCount() => new RandomState { Value = PcgRandomAlgorithm.InitializeState((ulong)System.Environment.TickCount) };
    }
}
