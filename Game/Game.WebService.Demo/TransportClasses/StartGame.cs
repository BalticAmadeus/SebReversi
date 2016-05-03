using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class StartGameReq : BaseReq
    {
        [DataMember]
        public int GameId;
    }

    [DataContract]
    public class StartGameResp : BaseResp
    {
        // default
    }
}
