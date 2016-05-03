using System.Runtime.Serialization;
using Game.WebService.Demo.TransportClasses;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class SetGameMapReq : BaseReq
    {
        [DataMember]
        public int GameId;

        [DataMember]
        public EnMapData MapData;
    }

    [DataContract]
    public class SetGameMapResp : BaseResp
    {
        // default
    }
}
