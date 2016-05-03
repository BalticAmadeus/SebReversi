using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class CompleteLoginReq : BaseReq
    {
        [DataMember] public string ChallengeResponse;
    }

    [DataContract]
    public class CompleteLoginResp : BaseResp
    {
        [DataMember] public int SessionId;
    }
}