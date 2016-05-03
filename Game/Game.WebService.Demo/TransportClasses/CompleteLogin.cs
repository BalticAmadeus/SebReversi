using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class CompleteLoginReq : BaseReq
    {
        [DataMember]
        public string ChallengeResponse;
    }

    [DataContract]
    public class CompleteLoginResp : BaseResp
    {
        [DataMember]
        public int SessionId;
    }
}
