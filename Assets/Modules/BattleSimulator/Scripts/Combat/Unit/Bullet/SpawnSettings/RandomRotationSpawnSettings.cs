using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Bullet.SpawnSettings
{
    public class RandomRotationSpawnSettings : IBulletSpawnSettings
    {
        private static readonly RandomRotationSpawnSettings _instance = new();

        public static IBulletSpawnSettings Instance => _instance;
        public Vector2 GetOffset(int bulletIndex) => Vector2.zero;
        public float GetRotation(int bulletIndex) => Random.Range(0, 360);
    }
}