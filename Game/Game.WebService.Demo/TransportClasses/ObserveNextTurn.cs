using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class ObserveNextTurnReq : BaseReq
    {
        [DataMember]
        public int ObserverId;

        [DataMember]
        public int GameId;
    }

    [DataContract]
    public class ObserveNextTurnResp : BaseResp
    {
        [DataMember]
        public EnObsGameInfo GameInfo;

        [DataMember]
        public EnObsTurnInfo TurnInfo;
    }
}