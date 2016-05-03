using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class CreatePlayerReq : BaseReq
    {
        // empty
    }

    [DataContract]
    public class CreatePlayerResp : BaseResp
    {
        [DataMember] public int PlayerId;
    }
}