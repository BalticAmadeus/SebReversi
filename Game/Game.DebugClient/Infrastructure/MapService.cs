using System;
using System.Collections.Generic;

namespace Game.DebugClient.Infrastructure
{
    public class MapService : IMapService
    {
        public event MapChangedEventHandler MapChanged;
        public event CellChangedEventHandler CellChanged;
        public string[] Map { get; private set; }
        public IList<Player> Players { get; private set; }

        public void UpdateCell(int x, int y, string state, Player player)
        {
            Map[y] = Map[y].Substring(0, x) + Convert.ToChar(state) + Map[y].Substring(x + 1);
            Players[player.Index] = player;

            CellChanged?.Invoke(this, new CellChangedEvent {State = state, X = x, Y = y});
        }

        public void UpdateMap(string[] map, List<Player> players)
        {
            Map = map;
            Players = players;

            MapChanged?.Invoke(this, new MapChangedEventArgs {Map = map});
        }
    }
}