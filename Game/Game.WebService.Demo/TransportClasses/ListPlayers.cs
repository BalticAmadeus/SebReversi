using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class ListPlayersReq : BaseReq
    {
        // empty
    }

    [DataContract]
    public class ListPlayersResp : BaseResp
    {
        [DataMember]
        public List<EnPlayerInfo> Players;
    }
}
