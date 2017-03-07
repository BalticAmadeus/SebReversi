using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WebService.Demo
{
    public class GameSolver
    {
        private MapData _map;
        private TileType _playerType;

        public GameSolver(PlayerReq req)
        {
            ParseMap(req.Map);
            _playerType = (req.YourIndex == 0) ? TileType.Player1 : TileType.Player2;
        }

        private void ParseMap(EnMapData map)
        {
            _map = new MapData();

            _map.Width = map.Width;
            _map.Height = map.Height;

            _map.Tiles = new TileType[_map.Height, _map.Width];
            for (int row = 0; row < _map.Height; row++)
            {
                ParseRow(row, map.Rows[row]);
            }
        }

        private void ParseRow(int row, string data)
        {
            for (int col = 0; col < _map.Width; col++)
            {
                _map.Tiles[row, col] = parseTile(data[col]);
            }
        }

        private TileType parseTile(char code)
        {
            switch (code)
            {
                case ' ':
                case '*':
                case '.':
                    return TileType.Empty;
                case '0':
                    return TileType.Player1;
                case '1':
                    return TileType.Player2;
                default:
                    throw new ApplicationException(string.Format("Invalid map tile character '{0}'", code.ToString()));
            }
        }

        public EnPoint GetTurn()
        {
            ReversiRules rules = new ReversiRules(_map);
            Point? turn = null;

            if (rules.GetFirstLegalMove(_playerType) != null)
            {
                Random rnd = new Random();
                while (true)
                {
                    turn = rules.GetLegalMove(_playerType, rnd.Next(0, 4));
                    if (turn != null)
                        break;
                }
            }

            if (turn != null)
                return new EnPoint()
                {
                    Row = turn.Value.Row,
                    Col = turn.Value.Col
                };
            return null;
        }

    }

    public class MapData : ICloneable
    {
        public int Width;
        public int Height;
        /// <summary>
        /// Zero-Based coordinates i.e. from [0, 0] to [Row, Col] ~ [Height-1, Width-1]
        /// </summary>
        public TileType[,] Tiles;

        public bool IsFilled
        {
            get
            {
                foreach (var t in Tiles)
                {
                    if (t == TileType.Empty) return false;
                }
                return true;
            }
        }

        public object Clone()
        {
            return new MapData
            {
                Width = this.Width,
                Height = this.Height,
                Tiles = (TileType[,])this.Tiles.Clone()
            };
        }

        public bool InBounds(Point point)
        {
            return point.Col >= 0
                && point.Row >= 0
                && point.Col < Width
                && point.Row < Height;
        }

        public void ChangeTile(Point point, TileType value)
        {
            if (!InBounds(point)) return;

            Tiles[point.Row, point.Col] = value;
        }

        public override string ToString()
        {
            var output = string.Empty;
            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    switch (Tiles[row, col])
                    {
                        case TileType.Empty:
                            output += "▯";
                            break;
                        case TileType.Player1:
                            output += "☻";
                            break;
                        case TileType.Player2:
                            output += "☺";
                            break;
                        default:
                            output += "E"; // error
                            break;
                    }
                }
                output += "\n";
            }
            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is MapData))
                return false;

            var other = (MapData)obj;

            if (string.Equals(this.ToString(), other.ToString()))
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    public enum TileType : byte
    {
        Empty = 0,
        Player1,
        Player2,
        IllegalMove
    }

    public struct Point
    {
        public int Row;
        public int Col;

        public Point(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                Point other = (Point)obj;
                return other.Row == this.Row && other.Col == this.Col;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Row.GetHashCode() + Col.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", Row, Col);
        }
    }

    public class ReversiRules
    {
        private const int Directions = 8;
        private readonly int[] _directionCol = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private readonly int[] _directionRow = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private readonly MapData _map;

        public ReversiRules(MapData map)
        {
            _map = map;
        }

        private TileType GetEnemy(TileType player)
        {
            if (player == TileType.Player1) return TileType.Player2;
            if (player == TileType.Player2) return TileType.Player1;

            throw new ArgumentOutOfRangeException(nameof(player));
        }

        public Point? GetFirstLegalMove(TileType player)
        {
            for (int x = 0; x < _map.Height; x++)
                for (int y = 0; y < _map.Width; y++)
                {
                    if (_map.Tiles[x, y] != TileType.Empty) continue;
                    var move = new Point(x, y);
                    if (IsLegalMove(move, player)) return move;
                }
            return null;
        }

        public Point? GetLegalMove(TileType player, int moveNumber)
        {
            for (int x = 0; x < _map.Height; x++)
                for (int y = 0; y < _map.Width; y++)
                {
                    if (_map.Tiles[x, y] != TileType.Empty) continue;
                    var move = new Point(x, y);
                    if (IsLegalMove(move, player))
                    {
                        if (moveNumber-- > 0) continue;
                        return move;
                    }
                }
            return null;
        }

        public bool IsLegalMove(Point move, TileType player)
        {
            if (!_map.InBounds(move)) return false;
            if (_map.Tiles[move.Row, move.Col] != TileType.Empty) return false;

            var enemy = GetEnemy(player);

            for (int direction = 0; direction < Directions; direction++)
            {
                bool sawEnemy = false;

                int row = move.Row,
                    col = move.Col;

                for (int m = 0; m < _map.Height; m++)
                {
                    row += _directionRow[direction];
                    col += _directionCol[direction];

                    if (!_map.InBounds(new Point(row, col))) break;

                    var occupation = _map.Tiles[row, col];

                    if (occupation == TileType.Empty) break;

                    if (occupation == enemy) sawEnemy = true;
                    else if (occupation == player)
                    {
                        if (sawEnemy) return true;
                        else break;
                    }
                    else
                        throw new Exception($"Invalid occupation at [{row}, {col}]");
                }
            }
            return false;
        }

        public List<IMapPoint> GetMapChanges(Point move, TileType player)
        {
            var changes = new List<IMapPoint>();

            var enemy = GetEnemy(player);

            var flip = new List<IMapPoint>();

            for (int direction = 0; direction < Directions; direction++)
            {
                int row = move.Row,
                    col = move.Col;

                for (int m = 0; m < _map.Height; m++)
                {
                    row += _directionRow[direction];
                    col += _directionCol[direction];

                    if (!_map.InBounds(new Point(row, col))) break;

                    var occupation = _map.Tiles[row, col];

                    if (occupation == TileType.Empty) break;

                    if (occupation == enemy)
                    {
                        flip.Add(new ReversiPoint(new Point(row, col), player));
                    }
                    else if (occupation == player)
                    {
                        changes.AddRange(flip);
                        break;
                    }
                }

                flip.Clear();
            }

            changes.Add(new ReversiPoint(new Point(move.Row, move.Col), player));

            return changes;
        }

        public int GetScore(TileType player)
        {
            var score = 0;
            for (var row = 0; row < _map.Height; row++)
            {
                for (var col = 0; col < _map.Width; col++)
                {
                    if (_map.Tiles[row, col] == player) score++;
                }
            }
            return score;
        }
    }

    public interface IMapPoint
    {
        Point Point { get; }
        TileType PointType { get; }
    }

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
