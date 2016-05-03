using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class CreatePlayerReq : BaseReq
    {
        // empty
    }

    [DataContract]
    public class CreatePlayerResp : BaseResp
    {
        [DataMember]
        public int PlayerId;
    }
}
