using System;
using System.Collections.Generic;

namespace Game.DebugClient.Infrastructure
{
    public interface IMapService
    {
        string[] Map { get; }
        IList<Player> Players { get; }
        event CellChangedEventHandler CellChanged;
        event MapChangedEventHandler MapChanged;

        void UpdateCell(int x, int y, string state, Player player);
        void UpdateMap(string[] map, List<Player> players);
    }

    public delegate void MapChangedEventHandler(object sender, MapChangedEventArgs args);

    public class MapChangedEventArgs
    {
        public string[] Map { get; set; }
    }

    public class CellChangedEvent : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string State { get; set; }
    }

    public delegate void CellChangedEventHandler(object sender, CellChangedEvent args);

    public class Player
    {
        public int Index { get; set; }
        public string Condition { get; set; }
    }
}