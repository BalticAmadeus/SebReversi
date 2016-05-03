using GameLogic.Reversi;
using System.Collections.Generic;

namespace GameLogic
{
    public interface IGameRules
    {
        Point? GetFirstLegalMove(TileType player);

        bool IsLegalMove(Point move, TileType player);

        List<IMapPoint> GetMapChanges(Point move, TileType player);

        int GetScore(TileType player);
    }
}
