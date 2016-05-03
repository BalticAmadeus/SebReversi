using System;
using System.Collections.Generic;

namespace GameLogic.Reversi
{
    public class ReversiRules : IGameRules
    {
        private const int Directions = 8;
        private readonly int[] _directionCol = { -1,  0,  1, -1,  1, -1,  0,  1 };
        private readonly int[] _directionRow = { -1, -1, -1,  0,  0,  1,  1,  1 };

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
}
