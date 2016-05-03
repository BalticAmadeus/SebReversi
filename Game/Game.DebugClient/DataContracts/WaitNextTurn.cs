using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class WaitNextTurnReq : BaseReq
    {
        [DataMember]
        public int PlayerId;

        [DataMember]
        public int RefTurn;
    }

    [DataContract]
    public class WaitNextTurnResp : BaseResp
    {
        [DataMember]
        public string FinishComment;

        [DataMember]
        public string FinishCondition;

        [DataMember]
        public bool GameFinished;

        [DataMember]
        public bool TurnComplete;

        [DataMember]
        public bool YourTurn;
    }
}