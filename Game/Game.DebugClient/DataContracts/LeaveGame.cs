using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class LeaveGameReq : BaseReq
    {
        [DataMember] public int PlayerId;
    }

    [DataContract]
    public class LeaveGameResp : BaseResp
    {
        // default
    }
}