using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Game.WebService.Demo;

namespace Game.WebService
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class DemoService : IDemoService
    {
        public PlayerResp DemoMove(PlayerReq req)
        {
            var solver = new GameSolver(req);
            var resp = new PlayerResp
            {
                Turn = solver.GetTurn()
            };
            resp.Pass = (resp.Turn == null);
            return resp;
        }
    }
}
