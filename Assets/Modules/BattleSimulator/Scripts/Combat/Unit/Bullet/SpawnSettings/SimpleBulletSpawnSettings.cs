using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Bullet.SpawnSettings
{
    public class SimpleBulletSpawnSettings : IBulletSpawnSettings
    {
        public SimpleBulletSpawnSettings(Vector2 offset, float rotation)
        {
            _offset = offset.ToData();
            _rotation = rotation;
        }

        public static SimpleBulletSpawnSettings FromFloatOffset(float offset)
        {
            return new SimpleBulletSpawnSettings(new Vector2(offset, 0), 0);
        }

        public Vector2Data GetOffset(int bulletIndex)
        {
            return _offset;
        }

        public float GetRotation(int bulletIndex)
        {
            return _rotation;
        }

        private readonly Vector2Data _offset;
        private readonly float _rotation;
    }
}