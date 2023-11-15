using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
    // TODO: move formulas to database
    public static class ModificationQualityExtensions
    {
        public static T ToValue<T>(this ModificationQuality quality, T n3, T n2, T n1, T p1, T p2, T p3)
        {
            switch (quality)
            {
                case ModificationQuality.N3:
                    return n3;
                case ModificationQuality.N2:
                    return n2;
                case ModificationQuality.N1:
                    return n1;
                case ModificationQuality.P1:
                    return p1;
                case ModificationQuality.P2:
                    return p2;
                case ModificationQuality.P3:
                    return p3;
                default:
                    throw new System.ArgumentException(nameof(quality));
            }
        }
    }
}
