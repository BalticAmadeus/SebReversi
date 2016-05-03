using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class EnPlayerState
    {
        [DataMember] public string Comment;
        [DataMember] public string Condition;

        [DataMember] public EnPoint Position;
    }
}