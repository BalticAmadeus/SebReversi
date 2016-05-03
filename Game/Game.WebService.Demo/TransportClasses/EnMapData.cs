using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using GameLogic;

namespace Game.WebService.Demo.TransportClasses
{
    [DataContract]
    public class EnMapData
    {
        [DataMember]
        public int Width;

        [DataMember]
        public int Height;

        [DataMember]
        public List<string> Rows;

        public EnMapData()
        {
            // default
        }

        public EnMapData(MapData md)
        {
            Width = md.Width;
            Height = md.Height;
            Rows = new List<string>();
            for (int row = 0; row < Height; row++)
            {
                Rows.Add(BuildRow(md, row));
            }
        }

        public MapData ToMapData()
        {
            MapData md = new MapData();
            md.Width = Width;
            md.Height = Height;
            if (md.Width > Settings.MapSizeLimit || md.Height > Settings.MapSizeLimit)
                throw new ApplicationException("Map too big");
            md.Tiles = new TileType[Height, Width];
            for (int row = 0; row < Height; row++)
            {
                ParseRow(row, Rows[row], md);
            }
            return md;
        }

        private void ParseRow(int row, string data, MapData md)
        {
            for (int col = 0; col < Width; col++)
            {
                md.Tiles[row, col] = parseTile(data[col]);
            }
        }

        private TileType parseTile(char code)
        {
            switch (code)
            {
                case '#':
                    return TileType.Block;
                case ' ':
                case '.':
                    return TileType.Empty;
                case '0':
                    return TileType.Head0;
                case '1':
                    return TileType.Head1;
                case '2':
                    return TileType.Head2;
                case '3':
                    return TileType.Head3;
                case '4':
                    return TileType.Head4;
                case '5':
                    return TileType.Head5;
                case '6':
                    return TileType.Head6;
                case '7':
                    return TileType.Head7;
                default:
                    throw new ApplicationException(string.Format("Invalid map tile character '{0}'", code.ToString()));
            }
        }

        public static string BuildRow(MapData md, int row)
        {
            StringBuilder sb = new StringBuilder();
            for (int col = 0; col < md.Width; col++)
            {
                sb.Append(BuildTile(md.Tiles[row, col]));
            }
            return sb.ToString();
        }

        public static char BuildTile(TileType tile)
        {
            switch (tile)
            {
                case TileType.Block:
                    return '#';
                case TileType.Empty:
                    return '.';
                case TileType.Head0:
                case TileType.Head1:
                case TileType.Head2:
                case TileType.Head3:
                case TileType.Head4:
                case TileType.Head5:
                case TileType.Head6:
                case TileType.Head7:
                    return (char)('0' + (tile - TileType.Head0));
                case TileType.Body0:
                case TileType.Body1:
                case TileType.Body2:
                case TileType.Body3:
                case TileType.Body4:
                case TileType.Body5:
                case TileType.Body6:
                case TileType.Body7:
                    return (char)('a' + (tile - TileType.Body0));
                default:
                    return '#';
            }
        }
    }
}
