using System;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using GameLogic;
using GameLogic.Reversi;

namespace Game.DemoClient
{
    public class GameFlowWrapper
    {
        public bool InitLogin()
        {
            Console.WriteLine(@"Init login...");
            var req = new InitLoginReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                }
            };

            var initLoginResp = Program.ServiceCallInvoker.InvokeAsync<InitLoginReq, InitLoginResp>(Settings.ServerUrl.TrimEnd('/') + "/json/InitLogin", req).Result;

            if (initLoginResp.IsOk())
            {
                Settings.Challenge = initLoginResp.Challenge;
                return true;
            }
            return false;
        }

        public bool CompleteLogin()
        {
            Console.WriteLine(@"Complete login...");

            var req = new CompleteLoginReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                ChallengeResponse = Settings.ChallengeResponse
            };

            var completeLoginResp = Program.ServiceCallInvoker.InvokeAsync<CompleteLoginReq, CompleteLoginResp>(Settings.ServerUrl.TrimEnd('/') + "/json/CompleteLogin", req).Result;
            Settings.SequenceNumber++;

            if (completeLoginResp.IsOk())
            {
                Settings.SessionId = completeLoginResp.SessionId;
                return true;
            }
            return false;

        }

        public bool CreatePlayer()
        {
            Console.WriteLine(@"Create player...");
            var req = new CreatePlayerReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                }
            };

            var createPlayerResp = Program.ServiceCallInvoker.InvokeAsync<CreatePlayerReq, CreatePlayerResp>(Settings.ServerUrl.TrimEnd('/') + "/json/CreatePlayer", req).Result;
            Settings.SequenceNumber++;

            if (createPlayerResp.IsOk())
            {
                Settings.PlayerId = createPlayerResp.PlayerId;
                return true;
            }

            return false;
        }

        public bool WaitGameStart()
        {
            Console.WriteLine(@"Wait game start...");
            var req = new WaitGameStartReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                PlayerId = Settings.PlayerId
            };

            while (true)
            {
                req.Auth.AuthCode = Settings.AuthCode;
                req.Auth.SequenceNumber = Settings.SequenceNumber;

                var waitGameStartResp = Program.ServiceCallInvoker.InvokeAsync<WaitGameStartReq, WaitGameStartResp>(Settings.ServerUrl.TrimEnd('/') + "/json/WaitGameStart", req).Result;
                Settings.SequenceNumber++;

                if (waitGameStartResp.IsOk())
                {
                    if (waitGameStartResp.GameId != -1)
                    {
                        Settings.GameId = waitGameStartResp.GameId;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool WaitNextTurn()
        {
            var req = new WaitNextTurnReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                PlayerId = Settings.PlayerId,
                RefTurn = Settings.Turn
            };

            while (true)
            {
                req.Auth.AuthCode = Settings.AuthCode;
                req.Auth.SequenceNumber = Settings.SequenceNumber;
                req.RefTurn = Settings.Turn;

                var waitNextTurnResp = Program.ServiceCallInvoker.InvokeAsync<WaitNextTurnReq, WaitNextTurnResp>(Settings.ServerUrl.TrimEnd('/') + "/json/WaitNextTurn", req).Result;
                Settings.SequenceNumber++;

                if (waitNextTurnResp.IsOk())
                {
                    if (waitNextTurnResp.GameFinished)
                        return false;

                    if (waitNextTurnResp.YourTurn && waitNextTurnResp.TurnComplete)
                        return true;
                    if (waitNextTurnResp.YourTurn == false && waitNextTurnResp.TurnComplete)
                    {
                        Settings.Turn++;
                        continue;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool GetPlayerView()
        {
            var req = new GetPlayerViewReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                PlayerId = Settings.PlayerId
            };

            var getPlayerViewResp = Program.ServiceCallInvoker.InvokeAsync<GetPlayerViewReq, GetPlayerViewResp>(Settings.ServerUrl.TrimEnd('/') + "/json/GetPlayerView", req).Result;
            Settings.SequenceNumber++;

            if (getPlayerViewResp.IsOk())
            {
                Settings.Turn = getPlayerViewResp.Turn;
                Settings.MapData = MapConverter.ToMapData(getPlayerViewResp.Map);
                Settings.PlayerType = (TileType)getPlayerViewResp.Index + 1;
                return true;
            }

            return false;
        }

        public bool PerformMove()
        {
            var req = new PerformMoveReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                PlayerId = Settings.PlayerId,
                Turn = GetTurn()
            };

            if (req.Turn == null)
                req.Pass = true;

            var performMoveResp = Program.ServiceCallInvoker.InvokeAsync<PerformMoveReq, PerformMoveResp>(Settings.ServerUrl.TrimEnd('/') + "/json/PerformMove", req).Result;
            Settings.SequenceNumber++;

            if (performMoveResp.IsOk())
            {
                return true;
            }
            return false;
        }

        public bool LeaveGame()
        {
            var req = new LeaveGameReq
            {
                Auth = new ReqAuth
                {
                    TeamName = Settings.TeamName,
                    AuthCode = Settings.AuthCode,
                    ClientName = Settings.UserName,
                    SequenceNumber = Settings.SequenceNumber,
                    SessionId = Settings.SessionId
                },
                PlayerId = Settings.PlayerId
            };

            var leaveGameResp = Program.ServiceCallInvoker.InvokeAsync<LeaveGameReq, LeaveGameResp>(Settings.ServerUrl.TrimEnd('/') + "/json/LeaveGame", req).Result;
            Settings.SequenceNumber++;

            return leaveGameResp.IsOk();
        }

        private EnPoint GetTurn()
        {
            ReversiRules rules = new ReversiRules(Settings.MapData);
            Point? turn = null;

            if (rules.GetFirstLegalMove(Settings.PlayerType) != null)
            {
                while (true)
                {
                    Random rnd = new Random((int)DateTime.Now.Ticks);
                    turn = rules.GetLegalMove(Settings.PlayerType, rnd.Next(0, 4));
                    if (turn != null)
                        break;
                }
            }

            if (turn != null)
                return new EnPoint()
                {
                    Row = turn.Value.Row,
                    Col = turn.Value.Col
                };
            return null;
        }
    }
}
