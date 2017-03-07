using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.DemoClient
{
    public class PlayerReq
    {
        public string GameUid;
        public int Turn;

        public int YourIndex;
        public List<EnPlayerState> Players;
        public EnMapData Map;
    }

    public class PlayerResp
    {
        public EnPoint Turn;
        public bool Pass;
    }

    public class EnPlayerState
    {
        public string Condition;
        public string Comment;
        public int Score;
    }

    public class EnMapData
    {
        public int Width;
        public int Height;
        public List<string> Rows;
    }

    public class EnPoint
    {
        public int Row;
        public int Col;
    }
}
