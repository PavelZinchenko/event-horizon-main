using System;
using GameDatabase.Model;
using UnityEngine;

namespace GameDatabase.DataModel
{
    public partial class BulletTrigger_SpawnBullet : IBulletSpawnSettings
    {
        public Vector2 GetOffset(int bulletIndex)
        {
            return new Vector2(OffsetX.Invoke(bulletIndex), OffsetY.Invoke(bulletIndex));
        }

        public float GetRotation(int bulletIndex)
        {
            var rot = Rotation.Invoke(bulletIndex);
            return rot;
        }
    }
}