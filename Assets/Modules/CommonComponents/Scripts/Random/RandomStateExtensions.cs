namespace CommonComponents
{
    public static class RandomStateExtensions
    {
        private const double _toDouble01 = 1.0 / 4294967296.0;
        private const uint _maxFloatMantissa = 16777216;
        private const float _toFloat01 = 1.0f / _maxFloatMantissa;

        public static int Next(this ref RandomState state) => (int)(PcgRandomAlgorithm.NextUint(ref state.Value) & 0x7fffffff);
        public static uint NextUint(this ref RandomState state) => PcgRandomAlgorithm.NextUint(ref state.Value);
        public static float NextFloat(this ref RandomState state) => PcgRandomAlgorithm.NextUint(ref state.Value) % (_maxFloatMantissa + 1) * _toFloat01;
        public static double NextDouble(this ref RandomState state) => PcgRandomAlgorithm.NextUint(ref state.Value) * _toDouble01;

        public static float NextFloat(ref ulong state, ulong increment) => PcgRandomAlgorithm.NextUint(ref state, increment) % (_maxFloatMantissa + 1) * _toFloat01;
        public static double NextDouble(ref ulong state, ulong increment) => PcgRandomAlgorithm.NextUint(ref state, increment) * _toDouble01;
    }
}
