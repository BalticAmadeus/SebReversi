namespace Game.ClientHandlerNet
{
    public class InitLoginReq : BaseReq
    {
        // default
    }

    public class InitLoginResp : BaseResp
    {
        public string Challenge;
    }
}
