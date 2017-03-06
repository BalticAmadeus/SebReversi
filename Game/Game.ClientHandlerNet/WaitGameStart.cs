namespace Game.ClientHandlerNet
{
    public class WaitGameStartReq : BaseReq
    {
        public int PlayerId;
    }

    public class WaitGameStartResp : BaseResp
    {
        public int GameId;
    }
}
