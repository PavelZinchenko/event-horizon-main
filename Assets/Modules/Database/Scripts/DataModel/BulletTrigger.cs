using System;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
    public partial class BulletTrigger_SpawnBullet : IBulletSpawnSettings
    {
        public Vector2Data GetOffset(int bulletIndex)
        {
            return new Vector2Data(OffsetX.Invoke(bulletIndex), OffsetY.Invoke(bulletIndex));
        }

        public float GetRotation(int bulletIndex)
        {
            var rot = Rotation.Invoke(bulletIndex);
            return rot;
        }
    }
}