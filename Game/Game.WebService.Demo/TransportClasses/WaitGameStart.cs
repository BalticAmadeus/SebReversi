using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class WaitGameStartReq : BaseReq
    {
        [DataMember]
        public int PlayerId;
    }

    [DataContract]
    public class WaitGameStartResp : BaseResp
    {
        [DataMember]
        public int GameId;
    }
}
