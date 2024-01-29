using UnityEngine;

namespace GameDatabase.Model
{

    public interface IBulletSpawnSettings
    {
        public Vector2 GetOffset(int bulletIndex);
        public float GetRotation(int bulletIndex);
    }
}