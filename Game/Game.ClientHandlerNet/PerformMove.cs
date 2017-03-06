namespace Game.ClientHandlerNet
{
    public class PerformMoveReq : BaseReq
    {
        public int PlayerId;
        public EnPoint Turn;
        public bool Pass;
    }

    public class PerformMoveResp : BaseResp
    {
        // default
    }

    public class EnPoint
    {
        public int Row;
        public int Col;
    }
}
