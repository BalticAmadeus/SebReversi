using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class CreateObserverReq : BaseReq
    {
        // empty
    }

    [DataContract]
    public class CreateObserverResp : BaseResp
    {
        [DataMember]
        public int ObserverId;
    }
}