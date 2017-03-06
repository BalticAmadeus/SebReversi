using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.ClientHandlerNet
{
    public class PlayerReq
    {
        public string GameUid;
        public int Turn;

        public int YourIndex;
        public List<EnPlayerState> Players;
        public EnMapData Map;

        public PlayerReq(GetPlayerViewResp view)
        {
            //GameUid = ???; FIXME
            Turn = view.Turn;
            YourIndex = view.Index;
            Players = view.PlayerStates;
            Map = view.Map;
        }
    }

    public class PlayerResp
    {
        public EnPoint Turn;
        public bool Pass;

        public PerformMoveReq CreateMove(int playerId)
        {
            return new PerformMoveReq
            {
                PlayerId = playerId,
                Turn = Turn,
                Pass = Pass
            };
        }
    }
}
