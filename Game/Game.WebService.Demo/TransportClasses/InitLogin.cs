﻿using System.Runtime.Serialization;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class InitLoginReq : BaseReq
    {
        // default
    }

    [DataContract]
    public class InitLoginResp : BaseResp
    {
        [DataMember]
        public string Challenge;
    }
}
