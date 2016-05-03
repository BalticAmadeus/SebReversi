﻿using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Game.WebService.Demo.TransportClasses;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class EnMapChange
    {
        [DataMember]
        public EnPoint Position;

        [DataMember]
        public string Value;

        public EnMapChange()
        {
            // default
        }

        public EnMapChange(MapChange mc)
        {
            Position = new EnPoint(mc.Position);
            Value = EnMapData.BuildTile(mc.Value).ToString();
        }
    }
}