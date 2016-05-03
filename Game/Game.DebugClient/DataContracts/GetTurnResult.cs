using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class GetTurnResultReq : BaseReq
    {
        [DataMember] public int PlayerId;
    }

    [DataContract]
    public class GetTurnResultResp : BaseResp
    {
        [DataMember] public string GameState;

        [DataMember] public EnPlayerState[] PlayerStates;

        [DataMember] public int Turn;
    }
}