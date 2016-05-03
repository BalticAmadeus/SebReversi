using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class BaseReq
    {
        [DataMember] public ReqAuth Auth;
    }

    [DataContract]
    public class ReqAuth
    {
        [DataMember] public string AuthCode;

        [DataMember] public string ClientName;

        [DataMember] public int SequenceNumber;

        [DataMember] public int SessionId;

        [DataMember] public string TeamName;
    }

    [DataContract]
    public class BaseResp
    {
        [DataMember] public string Message;

        [DataMember] public string Status = "OK";
    }
}