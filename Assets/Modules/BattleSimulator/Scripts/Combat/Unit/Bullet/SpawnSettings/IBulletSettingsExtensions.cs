using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Bullet.SpawnSettings
{
    public static class IBulletSettingsExtensions
    {
        public static IBulletSpawnSettings WithExtraOffset(this IBulletSpawnSettings settings, Vector2 offset)
        {
            return offset == Vector2.zero ? settings : new BulletSpawnSettingsWrapper(settings, offset, 0);
        }
        
        public static IBulletSpawnSettings WithExtraRotation(this IBulletSpawnSettings settings, float rotation)
        {
            return rotation == 0 ? settings : new BulletSpawnSettingsWrapper(settings, Vector2.zero, 0);
        }
        
        public static IBulletSpawnSettings WithExtraOffsetAndRotation(this IBulletSpawnSettings settings, Vector2 offset, float rotation)
        {
            return offset == Vector2.zero && rotation == 0 ? settings : new BulletSpawnSettingsWrapper(settings, offset, 0);
        }
    }
}