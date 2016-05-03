﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class LeaveGameReq : BaseReq
    {
        [DataMember]
        public int PlayerId;
    }

    [DataContract]
    public class LeaveGameResp : BaseResp
    {
        // default
    }
}