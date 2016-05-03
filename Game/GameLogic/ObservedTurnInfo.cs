﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    public class ObservedTurnInfo
    {
        public int Turn;
        public GameState GameState;
        public PlayerStateInfo[] PlayerStates;
        public IMapPoint[] MapChanges;
    }
}
