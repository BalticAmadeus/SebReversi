namespace GameLogic.Reversi
{
    public class ReversiPoint : IMapPoint
    {
        readonly Point _point;
        readonly TileType _pointType;

        public ReversiPoint(Point point, TileType type)
        {
            _point = point;
            _pointType = type;
        }

        public Point Point { get { return _point; } }
        public TileType PointType { get { return _pointType; } }
    }
}
