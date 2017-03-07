using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Game.ClientHandlerNet
{
    public class ClientLoop
    {
        public delegate PlayerResp PlayerHandlerDelegate(PlayerReq req);

        private Profile _profile;
        private JsonWebServiceClient _webClient;
        private PlayerHandlerDelegate _playerHandler;
        private string _sessionLogDir;
        private int _sessionId;
        private int _sequenceNumber;
        private int _playerId;
        private int _gameId;
        private int _refTurn;

        public ClientLoop(Profile profile)
        {
            this._profile = profile;
            Console.WriteLine("Setting up session...");
            Console.WriteLine("serverUri: " + profile.ServerUri);
            Console.WriteLine("teamName: " + profile.TeamName);
            Console.WriteLine("clientName: " + profile.ClientName);
            Console.WriteLine("sessionDir: " + profile.SessionDir);

            switch (profile.ClientType)
            {
                case "exec":
                    _playerHandler = ExecPlayerHandler;
                    Console.WriteLine("clientType: exec " + profile.ExecName);
                    break;
                case "json":
                    _playerHandler = JsonPlayerHandler;
                    Console.WriteLine("clientType: json " + profile.JsonUri);
                    break;
                default:
                    throw new ApplicationException($"clientType not mapped - {profile.ClientType}");
            }

            _sessionId = new Random().Next(2147483640) + 1;
            _sequenceNumber = 1;
            _webClient = new JsonWebServiceClient();
        }

        public void Connect()
        {
            string challenge = InitLogin();
            string response = GetAuthCode(GetAuthCode(challenge));
            _sessionId = CompleteLogin(response);

            Console.WriteLine("sessionId: " + _sessionId);
            CreatePlayer();
            _sessionLogDir = Path.Combine(_profile.SessionDir, _sessionId.ToString());
            if (_profile.ClientType == "exec")
            {
                Directory.CreateDirectory(_sessionLogDir);
                Console.WriteLine($"Input/output files go to {_sessionLogDir}");
            }
        }

        public void DoLoop()
        {
            while (true)
            {
                _gameId = -1;
                while (_gameId < 0)
                {
                    _gameId = WaitGameStart();
                }
                Console.WriteLine($"Joined Game: GameId={_gameId}");
                _refTurn = 0;
                while (true)
                {
                    var turnResp = WaitNextTurn();
                    if (!turnResp.TurnComplete)
                        continue;
                    if (turnResp.GameFinished)
                    {
                        Console.WriteLine($"Game finished: {turnResp.FinishCondition} {turnResp.FinishComment}");
                        LeaveGame();
                        break;
                    }
                    var viewResp = GetPlayerView();
                    _refTurn = viewResp.Turn;
                    if (!turnResp.YourTurn)
                        continue;
                    var playerReq = new PlayerReq(viewResp);
                    var playerResp = _playerHandler(playerReq);
                    PerformMove(playerResp.CreateMove(_playerId));
                }
            }
        }

        private string InitLogin()
        {
            var resp = PerformCall<InitLoginResp>("InitLogin", new InitLoginReq(), true);
            return resp.Challenge;
        }

        private int CompleteLogin(string response)
        {
            var resp = PerformCall<CompleteLoginResp>("CompleteLogin", new CompleteLoginReq { ChallengeResponse = response }, true);
            return resp.SessionId;
        }

        private void CreatePlayer()
        {
            var resp = PerformCall<CreatePlayerResp>("CreatePlayer", new CreatePlayerReq());
            _playerId = resp.PlayerId;
            Console.WriteLine($"CreatePlayer: PlayerId={_playerId}");
        }

        private int WaitGameStart()
        {
            var resp = PerformCall<WaitGameStartResp>("WaitGameStart", new WaitGameStartReq { PlayerId = _playerId });
            return resp.GameId;
        }

        private WaitNextTurnResp WaitNextTurn()
        {
            var resp = PerformCall<WaitNextTurnResp>("WaitNextTurn", new WaitNextTurnReq { PlayerId = _playerId, RefTurn = _refTurn });
            return resp;
        }

        private GetPlayerViewResp GetPlayerView()
        {
            var resp = PerformCall<GetPlayerViewResp>("GetPlayerView", new GetPlayerViewReq { PlayerId = _playerId });
            return resp;
        }

        private void PerformMove(PerformMoveReq req)
        {
            PerformCall<PerformMoveResp>("PerformMove", req);
        }

        private void LeaveGame()
        {
            try
            {
                PerformCall<LeaveGameResp>("LeaveGame", new LeaveGameReq { PlayerId = _playerId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LeaveGame failed (ignoring): {ex.GetType().FullName} {ex.Message}");
            }
        }

        private T PerformCall<T>(string methodName, BaseReq req, bool isLoginCall = false)
            where T : BaseResp
        {
            req.Auth = new ReqAuth
            {
                TeamName = _profile.TeamName,
                ClientName = _profile.ClientName,
                SessionId = (!isLoginCall) ? _sessionId : 0,
                SequenceNumber = (!isLoginCall) ? _sequenceNumber : 0,
            };
            req.Auth.AuthCode = GetAuthCode(req.Auth.TeamName + ":" + req.Auth.ClientName + ":" + req.Auth.SessionId + ":" + req.Auth.SequenceNumber);

            _sequenceNumber++;

            var url = _profile.ServerUri + "/json/" + methodName;

            var resp = _webClient.Post<T>(url, req);

            if (resp.Status != "OK")
                throw new ApplicationException($"{methodName} returned {resp.Status} {resp.Message}");

            return resp;
        }

        private string GetAuthCode(string rawData)
        {
            var sha1Managed = HashAlgorithm.Create("SHA1");
            if (sha1Managed == null)
                throw new ArgumentException("SHA1 algorithm type is not supported.");

            var encoding = Encoding.UTF8;
            var bytes = encoding.GetBytes(rawData + _profile.TeamPassword);

            return BitConverter.ToString(sha1Managed.ComputeHash(bytes)).Replace("-", "").ToLower();
        }

        private PlayerResp ExecPlayerHandler(PlayerReq req)
        {
            string inputFile = Path.Combine(_sessionLogDir, $"{req.GameUid}_{_refTurn}_in.json");
            string outputFile = Path.Combine(_sessionLogDir, $"{req.GameUid}_{_refTurn}_out.json");
            string inputStr = JsonConvert.SerializeObject(req);
            File.WriteAllText(inputFile, inputStr);

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = _profile.ExecName;
            startInfo.Arguments = $"\"{inputFile}\" \"{outputFile}\"";
            var child = Process.Start(startInfo);
            child.WaitForExit();
            string outputStr = File.ReadAllText(outputFile);
            var resp = JsonConvert.DeserializeObject<PlayerResp>(outputStr);
            return resp;
        }

        private PlayerResp JsonPlayerHandler(PlayerReq req)
        {
            return _webClient.Post<PlayerResp>(_profile.JsonUri, req);
        }
    }
}