using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.WebService.Demo.TransportClasses;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class StartObservingReq : BaseReq
    {
        [DataMember]
        public int ObserverId;

        [DataMember]
        public int GameId;
    }

    [DataContract]
    public class StartObservingResp : BaseResp
    {
        [DataMember]
        public EnGameDetails GameDetails;

        [DataMember]
        public int Turn;

        [DataMember]
        public EnMapData MapData;

        [DataMember]
        public EnPlayerState[] PlayerStates;
    }
}