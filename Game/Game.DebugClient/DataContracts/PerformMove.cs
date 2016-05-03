using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class PerformMoveReq : BaseReq
    {
        [DataMember]
        public int PlayerId;

        [DataMember]
        public EnPoint Turn;

        [DataMember]
        public bool Pass;
    }

    [DataContract]
    public class PerformMoveResp : BaseResp
    {
        // default
    }
}