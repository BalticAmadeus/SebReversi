using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.TransportClasses
{
    [DataContract]
    public class EnObsGameInfo
    {
        [DataMember]
        public int GameId;

        [DataMember]
        public string GameState;

        [DataMember]
        public int QueuedTurns;

        public EnObsGameInfo()
        {
            // default
        }

        public EnObsGameInfo(ObservedGameInfo gi)
        {
            GameId = gi.GameId;
            GameState = gi.GameState.ToString();
            QueuedTurns = gi.QueuedTurns;
        }
    }
}