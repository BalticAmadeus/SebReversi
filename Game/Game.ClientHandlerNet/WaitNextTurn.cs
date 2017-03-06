namespace Game.ClientHandlerNet
{
    public class WaitNextTurnReq : BaseReq
    {
        public int PlayerId;
        public int RefTurn;
    }

    public class WaitNextTurnResp : BaseResp
    {
        public bool TurnComplete;
        public bool GameFinished;
        public string FinishCondition;
        public string FinishComment;
        public bool YourTurn;
    }
}
