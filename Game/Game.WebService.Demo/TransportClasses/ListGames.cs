using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class ListGamesReq : BaseReq
    {
        // empty
    }

    [DataContract]
    public class ListGamesResp : BaseResp
    {
        [DataMember]
        public List<EnGameInfo> Games;
    }
}
