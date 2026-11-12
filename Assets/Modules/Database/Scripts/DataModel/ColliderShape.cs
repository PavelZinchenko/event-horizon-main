namespace GameDatabase.DataModel
{
    public partial class ColliderShape
    {
        // 1 point is a circle and 2 points a capsule, both rounded by Radius. 3 or more points are an outline
        // without a radius: the physics library rounds only capsule cores.
        public bool IsValid
        {
            get
            {
                switch (Points.Count)
                {
                    case 0:
                        return false;
                    case 1:
                        return Radius > 0f;
                    case 2:
                        return Radius > 0f && (Points[0].x != Points[1].x || Points[0].y != Points[1].y);
                    default:
                        return Radius == 0f;
                }
            }
        }
    }
}
