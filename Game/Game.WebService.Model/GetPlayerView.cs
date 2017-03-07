using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.WebService.Model
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
        public string GameUid;

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

        [DataMember]
        public bool YourTurn { get; set; }
    }
}
