using System.Linq;
using System.ServiceModel;
using System.Threading;
using Game.WebService.MapConverters;
using GameLogic;
using Game.WebService.Model;
using GameLogic.UserManagement;

namespace Game.WebService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientService : ServiceBase, IClientService
    {
        public InitLoginResp InitLogin(InitLoginReq req)
        {
            return HandleServiceCall(req, new InitLoginResp(), InitLoginImpl, new SessionAuthOptions { IsLoginFlow = true });
        }

        private void InitLoginImpl(InitLoginReq req, InitLoginResp resp)
        {
            SessionChallenge challenge = Server.SessionManager.CreateChallenge(req.Auth.GetClientCode());
            resp.Challenge = challenge.Challenge;
        }

        public CompleteLoginResp CompleteLogin(CompleteLoginReq req)
        {
            return HandleServiceCall(req, new CompleteLoginResp(), CompleteLoginImpl, new SessionAuthOptions { IsLoginFlow = true });
        }

        private void CompleteLoginImpl(CompleteLoginReq req, CompleteLoginResp resp)
        {
            ClientSession session = Server.SessionManager.CreateSession(req.Auth.GetClientCode(), req.ChallengeResponse);
            resp.SessionId = session.SessionId;
        }


        public CreatePlayerResp CreatePlayer(CreatePlayerReq req)
        {
            return HandleServiceCall(req, new CreatePlayerResp(), CreatePlayerImpl);
        }

        private void CreatePlayerImpl(CreatePlayerReq req, CreatePlayerResp resp)
        {
            Team team = Server.TeamRegistry.GetTeam(req.Auth.TeamName);
            PlayerInfo player = Server.GameManager.CreatePlayer(team, req.Auth.ClientName);
            resp.PlayerId = player.PlayerId;
        }


        public WaitGameStartResp WaitGameStart(WaitGameStartReq req)
        {
            return HandleServiceCall(req, new WaitGameStartResp(), WaitGameStartImpl);
        }

        private void WaitGameStartImpl(WaitGameStartReq req, WaitGameStartResp resp)
        {
            GameInfo game = Server.GameManager.WaitGameStart(req.PlayerId, req.Auth.GetClientCode(), new PermissionPlayerAccessByClient());
            if (game == null)
                resp.GameId = -1;
            else
                resp.GameId = game.GameId;
        }


        public GetPlayerViewResp GetPlayerView(GetPlayerViewReq req)
        {
            return HandleServiceCall(req, new GetPlayerViewResp(), GetPlayerViewImpl);
        }

        private void GetPlayerViewImpl(GetPlayerViewReq req, GetPlayerViewResp resp)
        {
            GameViewInfo gv = Server.GameManager.GetPlayerView(req.PlayerId, req.Auth.GetClientCode(), new PermissionPlayerAccessByClient());
            resp.GameUid = gv.GameUid.ToString();
            resp.Index = gv.PlayerIndex;
            resp.GameState = gv.GameState.ToString();
            resp.Turn = gv.Turn;
            resp.YourTurn = gv.YourTurn;
            resp.PlayerStates = gv.PlayerStates.Select(p => new EnPlayerState(p)).ToList();
            resp.Map = new ReversiMapConverter().Convert(new EnMapData(gv.Map));
        }

        public PerformMoveResp PerformMove(PerformMoveReq req)
        {
            return HandleServiceCall(req, new PerformMoveResp(), PerformMoveImpl);
        }

        private void PerformMoveImpl(PerformMoveReq req, PerformMoveResp resp)
        {
            Server.GameManager.PerformMove(req.PlayerId, req.Turn?.ToPoint() ?? new Point(-1, -1), req.Auth.GetClientCode(), req.Pass, new PermissionPlayerAccessByClient());
        }

        public WaitNextTurnResp WaitNextTurn(WaitNextTurnReq req)
        {
            return HandleServiceCall(req, new WaitNextTurnResp(), WaitNextTurnImpl);
        }

        private void WaitNextTurnImpl(WaitNextTurnReq req, WaitNextTurnResp resp)
        {
            WaitTurnInfo wi = Server.GameManager.WaitNextTurn(req.PlayerId, req.RefTurn, req.Auth.GetClientCode(), new PermissionPlayerAccessByClient());
            resp.TurnComplete = wi.TurnComplete;
            resp.GameFinished = wi.GameFinished;
            resp.FinishCondition = wi.FinishCondition.ToString();
            resp.FinishComment = wi.FinishComment;
            resp.YourTurn = wi.YourTurn;
            //if(resp.YourTurn) //Wait for animated turns in observers
            //    Thread.Sleep(2500);
        }


        public GetTurnResultResp GetTurnResult(GetTurnResultReq req)
        {
            return HandleServiceCall(req, new GetTurnResultResp(), GetTurnResultImpl);
        }

        private void GetTurnResultImpl(GetTurnResultReq req, GetTurnResultResp resp)
        {
            GameResultInfo gr = Server.GameManager.GetTurnResultForPlayer(req.PlayerId, req.Auth.GetClientCode(), new PermissionPlayerAccessByClient());
            resp.Turn = gr.Turn;
            resp.GameState = gr.GameState.ToString();
            resp.PlayerStates = gr.PlayerStates.Select(p => new EnPlayerState(p)).ToArray();
        }


        public LeaveGameResp LeaveGame(LeaveGameReq req)
        {
            return HandleServiceCall(req, new LeaveGameResp(), LeaveGameImpl);
        }

        private void LeaveGameImpl(LeaveGameReq req, LeaveGameResp resp)
        {
            Server.GameManager.LeaveGame(req.PlayerId, -1, req.Auth.GetClientCode(), new PermissionPlayerAccessByClient());
        }
    }
}
