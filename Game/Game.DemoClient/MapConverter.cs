using System;
using System.Text;
using Game.DebugClient.DataContracts;
using GameLogic;

namespace Game.DemoClient
{
   public static class MapConverter
   {
       private static EnMapData _map;

        public static MapData ToMapData(EnMapData map)
        {
            _map = map;

            MapData md = new MapData();
            md.Width = _map.Width;
            md.Height = _map.Height;
            
            md.Tiles = new TileType[_map.Height, _map.Width];
            for (int row = 0; row < _map.Height; row++)
            {
                ParseRow(row, _map.Rows[row], md);
            }

            return md;
        }

        private static void ParseRow(int row, string data, MapData md)
        {
            for (int col = 0; col < _map.Width; col++)
            {
                md.Tiles[row, col] = parseTile(data[col]);
            }
        }

        private static TileType parseTile(char code)
        {
            switch (code)
            {
                //case '#':
                //    return TileType.Block;
                case ' ':
                case '*':
                case '.':
                    return TileType.Empty;
                case '0':
                    return TileType.Player1;
                case '1':
                    return TileType.Player2;
                //case '2':
                //    return TileType.Head2;
                //case '3':
                //    return TileType.Head3;
                //case '4':
                //    return TileType.Head4;
                //case '5':
                //    return TileType.Head5;
                //case '6':
                //    return TileType.Head6;
                //case '7':
                //    return TileType.Head7;
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
                //case TileType.Block:
                //    return '#';

                case TileType.Player1:
                case TileType.Player2:
                    //case TileType.Head2:
                    //case TileType.Head3:
                    //case TileType.Head4:
                    //case TileType.Head5:
                    //case TileType.Head6:
                    //case TileType.Head7:
                    return (char)('0' + (tile - TileType.Player1));
                //case TileType.Body0:
                //case TileType.Body1:
                //case TileType.Body2:
                //case TileType.Body3:
                //case TileType.Body4:
                //case TileType.Body5:
                //case TileType.Body6:
                //case TileType.Body7:
                //    return (char)('a' + (tile - TileType.Body0));
                case TileType.Empty:
                default:
                    return '.';
                    //return '#';
            }
        }
    }
}
