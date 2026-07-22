namespace EditorDatabase.Model
{
    public struct Vector2
    {
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;

        public override string ToString()
        {
            return "[" + x + "," + y + "]";
        }

        public static readonly Vector2 Zero = new Vector2 {x = 0, y = 0};
    }
}
