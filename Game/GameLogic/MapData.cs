using System;
using System.Linq;

namespace GameLogic
{
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
                Tiles = (TileType[,]) this.Tiles.Clone()
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
}
