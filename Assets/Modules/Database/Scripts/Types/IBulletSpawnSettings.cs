namespace GameDatabase.Model
{

    public interface IBulletSpawnSettings
    {
        public Vector2Data GetOffset(int bulletIndex);
        public float GetRotation(int bulletIndex);
    }
}