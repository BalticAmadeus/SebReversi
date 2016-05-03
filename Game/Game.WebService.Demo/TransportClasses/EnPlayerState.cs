using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class EnPlayerState
    {
        [DataMember]
        public string Condition;

        [DataMember]
        public EnPoint Position;

        [DataMember]
        public string Comment;

        public EnPlayerState()
        {
            //default
        }

        public EnPlayerState(PlayerStateInfo ps)
        {
            Condition = ps.Condition.ToString();
            Position = new EnPoint(ps.Position);
            Comment = ps.Comment;
        }
    }
}