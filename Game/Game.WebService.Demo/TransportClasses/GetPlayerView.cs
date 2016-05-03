using System.Collections.Generic;
using System.Runtime.Serialization;
using Game.WebService.Demo.TransportClasses;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class GetPlayerViewReq : BaseReq
    {
        [DataMember]
        public int PlayerId;
    }

    [DataContract]
    public class GetPlayerViewResp : BaseResp
    {
        [DataMember]
        public int Index;

        [DataMember]
        public string GameState;

        [DataMember]
        public int Turn;

        [DataMember]
        public List<EnPlayerState> PlayerStates;

        [DataMember]
        public EnMapData Map;
    }
}
