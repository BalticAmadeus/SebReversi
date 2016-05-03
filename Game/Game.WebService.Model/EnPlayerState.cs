using GameLogic;
using System.Runtime.Serialization;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnPlayerState
    {
        [DataMember]
        public string Condition;

        //[IgnoreDataMember] //Ignored for reversi game
        [DataMember]
        public EnPoint Position;

        [DataMember]
        public string Comment;

        [DataMember]
        public int Score { get; set; }

        public EnPlayerState()
        {
            //default
        }

        public EnPlayerState(PlayerStateInfo ps)
        {
            Condition = ps.Condition.ToString();
            Position = new EnPoint(ps.Position);
            Comment = ps.Comment;
            Score = ps.Score;
        }
    }
}