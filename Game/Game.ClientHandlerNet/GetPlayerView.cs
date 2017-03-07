using System.Collections.Generic;

namespace Game.ClientHandlerNet
{
    public class GetPlayerViewReq : BaseReq
    {
        public int PlayerId;
    }

    public class GetPlayerViewResp : BaseResp
    {
        public string GameUid;
        public int Index;
        public string GameState;
        public int Turn;
        public List<EnPlayerState> PlayerStates;
        public EnMapData Map;
    }

    public class EnPlayerState
    {
        public string Condition;
        public string Comment;
        public int Score;
    }

    public class EnMapData
    {
        public int Width;
        public int Height;
        public List<string> Rows;
    }
}
