using System;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;

namespace Game.DemoClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"Game Demo Bot - ver. {Assembly.GetExecutingAssembly().GetName().Version}";
            if (args.Length != 2)
            {
                Console.WriteLine("USAGE: Game.DemoClient inputFileName outputFileName");
                return;
            }

            var playerReq = JsonConvert.DeserializeObject<PlayerReq>(File.ReadAllText(args[0]));
            var playerResp = PerformMove(playerReq);
            File.WriteAllText(args[1], JsonConvert.SerializeObject(playerResp));
        }

        private static PlayerResp PerformMove(PlayerReq req)
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
