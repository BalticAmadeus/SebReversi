namespace Game.ClientHandlerNet
{
    public class CompleteLoginReq : BaseReq
    {
        public string ChallengeResponse;
    }

    public class CompleteLoginResp : BaseResp
    {
        public int SessionId;
    }
}
