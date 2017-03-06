using System;

namespace Game.ClientHandlerNet
{
    public class BaseReq
    {
        public ReqAuth Auth;
    }

    public class ReqAuth
    {
        public string TeamName;
        public string ClientName;
        public int SessionId;
        public int SequenceNumber;
        public string AuthCode;
    }

    public class BaseResp
    {
        public string Status;
        public string Message;
    }
}
