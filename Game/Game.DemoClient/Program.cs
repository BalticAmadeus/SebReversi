using System;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Game.DemoClient
{
    public class Program
    {
        private delegate void HandlerMode(string[] args);
        private static HandlerMode _connectionToClientHandler;

        static void Main(string[] args)
        {
            Console.Title = $"Game Demo Bot - ver. {Assembly.GetExecutingAssembly().GetName().Version}";
            if (args.Length != 2 && args.Length != 0)
            {
                Console.WriteLine("USAGE: Game.DemoClient inputFileName outputFileName");
                return;
            }

            if (args.Length == 0)
            {
                _connectionToClientHandler = StdioConnectionHandler;
            }
            else
            {
                _connectionToClientHandler = ExecConnectionHandler;
            }

            _connectionToClientHandler(args);

            var playerReq = JsonConvert.DeserializeObject<PlayerReq>(File.ReadAllText(args[0]));
            var playerResp = PerformMove(playerReq);
            File.WriteAllText(args[1], JsonConvert.SerializeObject(playerResp));
        }

        static void ExecConnectionHandler(string[] args)
        {
            // Read player view
            string inputStr = File.ReadAllText(args[0], Encoding.UTF8);
            var playerView = JsonConvert.DeserializeObject<PlayerReq>(inputStr);
            // Do the logic
            var playerMove = PerformMove(playerView);
            // Write move
            string outputStr = JsonConvert.SerializeObject(playerMove);
            File.WriteAllText(args[1], outputStr, Encoding.UTF8);
        }

        static void StdioConnectionHandler(string[] args)
        {
            while (true)
            {
                var input = Console.ReadLine();
                var playerMove = PerformMove(JsonConvert.DeserializeObject<PlayerReq>(input));
                Console.WriteLine(JsonConvert.SerializeObject(playerMove));
            }
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
