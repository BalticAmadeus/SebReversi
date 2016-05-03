using System;
using System.Reflection;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Game.DebugClient.Utilites;

namespace Game.DemoClient
{
    public class Program
    {
        public static IServiceCallInvoker ServiceCallInvoker { get; private set; }

        static void Main(string[] args)
        {
            Console.Title = $"Game Demo Bot - ver. {Assembly.GetExecutingAssembly().GetName().Version}";
            ServiceCallInvoker = new ServiceCallInvoker(new Logger(), new JsonWebServiceClient());
            ReadParams(args);

            var gameFlowWrapper = new GameFlowWrapper();

            while (true)
            {
                var noFail = gameFlowWrapper.InitLogin();
                if (noFail == false)
                    break;

                noFail = gameFlowWrapper.CompleteLogin();
                if (noFail == false)
                    break;

                noFail = gameFlowWrapper.CreatePlayer();
                if (noFail == false)
                    break;

                noFail = gameFlowWrapper.WaitGameStart();
               
                while (noFail)
                {
                    noFail = gameFlowWrapper.WaitNextTurn();
                    if (noFail == false)
                        break;

                    noFail = gameFlowWrapper.GetPlayerView();
                    if (noFail == false)
                        break;

                    noFail = gameFlowWrapper.PerformMove();
                    if (noFail == false)
                        break;
                }

                gameFlowWrapper.LeaveGame();
                break;
            }

            Console.ReadKey(true);
        }



        private static void ReadParams(string[] args)
        {
            Console.WriteLine(@"Enter server url:");
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine(args[0]);
                Settings.ServerUrl = args[0];
            }
            else
            {
                Settings.ServerUrl = Console.ReadLine();
            }


            Console.WriteLine(@"Enter Team name:");
            if (args.Length > 1 && !string.IsNullOrWhiteSpace(args?[1]))
            {
                Console.WriteLine(args[1]);
                Settings.TeamName = args[1];
            }
            else
            {
                Settings.TeamName = Console.ReadLine();
            }

            Console.WriteLine(@"Enter UserName:");
            if (args.Length > 2 && !string.IsNullOrWhiteSpace(args?[2]))
            {
                if (args[2] == "[random]") args[2] = "random" + new Random().Next(100);
                Console.WriteLine(args[2]);
                Settings.UserName = args[2];
            }
            else
            {
                Settings.UserName = Console.ReadLine();
            }

            Console.WriteLine(@"Enter Password:");
            if (args.Length > 3 && !string.IsNullOrWhiteSpace(args?[3]))
            {
                Console.WriteLine(args[3]);
                Settings.Password = args[3];
            }
            else
            {
                Settings.Password = Console.ReadLine();
            }

        }
    }
}
