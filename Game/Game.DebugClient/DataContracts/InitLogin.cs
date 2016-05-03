using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class InitLoginReq : BaseReq
    {
        // default
    }

    [DataContract]
    public class InitLoginResp : BaseResp
    {
        [DataMember] public string Challenge;
    }
}