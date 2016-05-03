using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class WaitGameStartReq : BaseReq
    {
        [DataMember] public int PlayerId;
    }

    [DataContract]
    public class WaitGameStartResp : BaseResp
    {
        [DataMember] public int GameId;
    }
}