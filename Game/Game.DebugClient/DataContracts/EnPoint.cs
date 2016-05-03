using System.Runtime.Serialization;

namespace Game.DebugClient.DataContracts
{
    [DataContract]
    public class EnPoint
    {
        [DataMember] public int Col;
        [DataMember] public int Row;
    }
}