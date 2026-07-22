namespace GameDatabase.ShipLayout
{
    public struct MousePosition
    {
        public MousePosition(int x, int y, int cellIndex)
        {
            X = x;
            Y = y;
            CellIndex = cellIndex;
        }

        public readonly int X;
        public readonly int Y;
        public readonly int CellIndex;
    }
}
