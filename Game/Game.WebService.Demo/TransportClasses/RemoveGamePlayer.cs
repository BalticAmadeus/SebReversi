using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class RemoveGamePlayerReq : BaseReq
    {
        [DataMember]
        public int GameId;

        [DataMember]
        public int PlayerId;
    }

    [DataContract]
    public class RemoveGamePlayerResp : BaseResp
    {
        // default
    }
}
