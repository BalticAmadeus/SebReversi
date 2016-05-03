namespace GameLogic
{
    public class WaitTurnInfo
    {
        public int PlayerIndex = -1;
        public int Turn;

        public bool TurnComplete;
        public bool GameFinished;
        public PlayerCondition FinishCondition;
        public string FinishComment;

        public bool YourTurn
        {
            get
            {
                if (PlayerIndex == -1)
                    return false;

                if (PlayerIndex == 0)
                    return TurnComplete && Turn % 2 == 0;
                return TurnComplete && Turn % 2 != 0;
            }
        }
    }
}
