using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
    public static class BulletShapeExtensions
    {
        public static bool HasDirection(this BulletShape shape)
        {
            switch (shape)
            {
                case BulletShape.Spark:
                case BulletShape.Mine:
                    return false;
                case BulletShape.Projectile:
                case BulletShape.Rocket:
                case BulletShape.LaserBeam:
                case BulletShape.LightningBolt:
                case BulletShape.EnergyBeam:
                case BulletShape.Wave:
                case BulletShape.PiercingLaser:
                default:
                    return true;
            }
        }

		public static bool IsBeam(this BulletShape shape)
		{
			switch (shape)
			{
				case BulletShape.LaserBeam:
				case BulletShape.EnergyBeam:
                case BulletShape.LightningBolt:
                case BulletShape.PiercingLaser:
                    return true;
				default:
					return false;
			}
		}
	}
}
