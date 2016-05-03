using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class GetPlayerViewReq : BaseReq
    {
        [DataMember] public int PlayerId;
    }

    [DataContract]
    public class GetPlayerViewResp : BaseResp
    {
        [DataMember] public string GameState;

        [DataMember] public int Index;

        [DataMember] public EnMapData Map;

        [DataMember]
        public bool YourTurn { get; set; }

        [DataMember] public List<EnPlayerState> PlayerStates;

        [DataMember] public int Turn;
    }
}