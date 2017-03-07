using GameLogic.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameLogic
{
    /// <summary>
    /// Locking strategy for live game access (e.g. by players): _gameLock -> _liveLock or _gameLock, _liveLock
    /// </summary>
    public class GameManager
    {
        private Dictionary<int, GameModel> _games;
        private Dictionary<int, Player> _players;
        private Dictionary<int, Observer> _observers;
        private object _gameLock;
        private int nextGameId;
        private int nextPlayerId;
        private int nextObserverId;
        private long uidBase;
        private readonly IGameRoleManager _roleManager;

        public GameManager(IGameRoleManager roleManager)
        {
            _games = new Dictionary<int, GameModel>();
            _players = new Dictionary<int, Player>();
            _observers = new Dictionary<int, Observer>();
            _gameLock = new object();
            nextGameId = 1;
            nextPlayerId = 1;
            nextObserverId = 1;
            uidBase = DateTime.UtcNow.Ticks / 10000000 * 10000000;
            _roleManager = roleManager;
        }

        public GameInfo[] ListGames(Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                return _games.Values
                    .Where(g => _roleManager.HasAccess(permission, new object[] { team, g }))
                    .Select(g => new GameInfo(g))
                    .ToArray();
            }
        }

        public GameInfo CreateGame(Team owner, PermissionStrategy permissionCreateGameLimited, PermissionStrategy permissionCreateGame)
        {
            lock (_gameLock)
            {
                if (!_roleManager.HasAccess(permissionCreateGameLimited, new object[] { owner }))
                    throw new UnauthorizedAccessException("Create game");

                if (!_roleManager.HasAccess(permissionCreateGame, new object[] { owner }))
                {
                    int count = _games.Values.Count(g => g.Owner.Equals(owner));
                    if (count >= Settings.MaxGamesPerTeam)
                        throw new ApplicationException("Maximum limit of games per owner team has been reached");
                }
                var game = new GameModel(uidBase, nextGameId++, owner);
                _games[game.GameId] = game;
                return new GameInfo(game);
            }
        }

        public GameDetails GetGameDetails(int gameId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                return new GameDetails(game);
            }
        }

        public PlayerInfo[] ListPlayers(Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                return _players.Values
                    .Where(p => _roleManager.HasAccess(permission, new object[] { team, p }))
                    .Select(p => new PlayerInfo(p))
                    .ToArray();
            }
        }

        public PlayerInfo CreatePlayer(Team team, string name)
        {
            lock (_gameLock)
            {
                Player player = _players.Values.FirstOrDefault(p => p.Name == name && p.Team.Equals(team));
                if (player == null)
                {
                    player = new Player(nextPlayerId++, team, name);
                    _players[player.PlayerId] = player;
                }
                return new PlayerInfo(player);
            }
        }

        public ObserverInfo CreateObserver(Team team, string name)
        {
            lock (_gameLock)
            {
                Observer observer = _observers.Values.FirstOrDefault(p => p.Name == name && p.Team.Equals(team));
                if (observer == null)
                {
                    observer = new Observer(nextObserverId++, team, name);
                    _observers[observer.ObserverId] = observer;
                }
                return new ObserverInfo(observer);
            }
        }

        public GameInfo WaitGameStart(int playerId, ClientCode clientCode, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                for (int i = 0; i < 2; i++)
                {
                    Player player = getPlayer(playerId);
                    checkPlayerAccess(player, clientCode, permission);
                    GameModel game = player.Game;
                    if (game != null && game.State != GameState.Setup)
                        return new GameInfo(game);
                    if (i == 0)
                        Monitor.Wait(_gameLock, Settings.GameStartPollTimeoutMillis);
                }
                return null;
            }
        }

        public void AddGamePlayer(int gameId, int playerId, Team team, PermissionStrategy permissionGame, PermissionStrategy permissionPlayer)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permissionGame);
                Player player = getPlayer(playerId);
                checkPlayerAccess(player, team, permissionPlayer);
                game.AddPlayer(player);
            }
        }

        public void RemoveGamePlayer(int gameId, int playerId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                Player player = getPlayer(playerId);
                game.RemovePlayer(player);
            }
        }

        private GameModel getGame(int gameId)
        {
            GameModel game;
            if (!_games.TryGetValue(gameId, out game))
                throw new ApplicationException("Game not found");
            return game;
        }

        private Player getPlayer(int playerId)
        {
            Player player;
            if (!_players.TryGetValue(playerId, out player))
                throw new ApplicationException("Player not found");
            return player;
        }

        private Observer getObserver(int observerId, ClientCode clientCode)
        {
            Observer observer;
            if (!_observers.TryGetValue(observerId, out observer))
                throw new ApplicationException("Observer not found");
            if (observer.Name != clientCode.ClientName || observer.Team.Name != clientCode.TeamName)
                throw new UnauthorizedAccessException();
            return observer;
        }

        private void checkGameAccess(GameModel game, Team team, PermissionStrategy permission)
        {
            if (!_roleManager.HasAccess(permission, new object[] { team, game }))
                throw new UnauthorizedAccessException();
        }

        private void checkPlayerAccess(Player player, ClientCode clientCode, PermissionStrategy permission)
        {
            if (!_roleManager.HasAccess(permission, new object[] { player, clientCode }))
                throw new UnauthorizedAccessException();
        }

        private void checkPlayerAccess(Player player, Team team, PermissionStrategy permission)
        {
            if (!_roleManager.HasAccess(permission, new object[] { player, team }))
                throw new UnauthorizedAccessException();
        }

        public void SetGameMap(int gameId, MapData mapData, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                game.SetMap(mapData);
            }
        }

        public void StartGame(int gameId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                game.Start();
                Monitor.PulseAll(_gameLock);
            }
        }

        private GameModel accessLiveGame(int playerId, ClientCode clientCode, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                Player player = getPlayer(playerId);
                checkPlayerAccess(player, clientCode, permission);
                GameModel game = player.Game;
                if (game == null)
                    throw new ApplicationException("Player is not in a game");
                game.CheckRunState();
                return game;
            }
        }

        private void accessObservedGame(int observerId, int gameId, ClientCode clientCode, Team team, out Observer observer, out GameModel game, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                observer = getObserver(observerId, clientCode);
                game = getGame(gameId);
                if (team != null)
                    checkGameAccess(game, team, permission);
                game.CheckRunState();
            }
        }

        public GameViewInfo GetPlayerView(int playerId, ClientCode clientCode, PermissionStrategy permission)
        {
            GameModel game = accessLiveGame(playerId, clientCode, permission);
            return game.GetGameView(playerId);
        }

        public void PerformMove(int playerId, Point position, ClientCode clientCode, bool pass, PermissionStrategy permission)
        {
            GameModel game = accessLiveGame(playerId, clientCode, permission);
            game.PerformMove(playerId, position, pass);
        }

        public WaitTurnInfo WaitNextTurn(int playerId, int refTurn, ClientCode clientCode, PermissionStrategy permission)
        {
            GameModel game = accessLiveGame(playerId, clientCode, permission);
            WaitTurnInfo wi = game.CompletePlayerTurn(playerId, refTurn);

            if (wi.TurnComplete == false)
                game.WaitNextTurn(wi);

            if (wi.GameFinished)
                Thread.Sleep(Settings.LastWaitNextTurnSleepMillis);

            return wi;
        }

        public GameResultInfo GetTurnResultForPlayer(int playerId, ClientCode clientCode, PermissionStrategy permission)
        {
            GameModel game = accessLiveGame(playerId, clientCode, permission);
            return game.GetTurnResult();
        }

        public void DropPlayer(int playerId, int gameId, ClientCode clientCode, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                Player player = getPlayer(playerId);
                checkPlayerAccess(player, team, permission);
                DropPlayer(gameId, clientCode, player);
            }
        }

        public void LeaveGame(int playerId, int gameId, ClientCode clientCode, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                Player player = getPlayer(playerId);
                checkPlayerAccess(player, clientCode, permission);
                DropPlayer(gameId, clientCode, player);
            }
        }

        private static void DropPlayer(int gameId, ClientCode clientCode, Player player)
        {
            GameModel game = player.Game;
            if (game == null)
                throw new ApplicationException("Player is not in a game");
            if (gameId > 0 && game.GameId != gameId)
                throw new ApplicationException("Player is not in the specified game");
            game.DropPlayer(player, clientCode.ToString());
        }

        public void PauseGame(int gameId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                game.Pause();
            }
        }

        public void ResumeGame(int gameId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                game.Resume();
            }
        }

        public GameViewInfo StartObserving(int observerId, int gameId, ClientCode clientCode, Team team, PermissionStrategy permission)
        {
            Observer observer;
            GameModel game;
            accessObservedGame(observerId, gameId, clientCode, team, out observer, out game, permission);
            return game.StartObserving(observer);
        }

        public ObservedGameInfo ObserveNextTurn(int observerId, int gameId, ClientCode clientCode, PermissionStrategy permission)
        {
            Observer observer;
            GameModel game;
            accessObservedGame(observerId, gameId, clientCode, null, out observer, out game, permission);
            return game.ObserveNextTurn(observer);
        }

        public void DeleteGame(int gameId, Team team, PermissionStrategy permission)
        {
            lock (_gameLock)
            {
                GameModel game = getGame(gameId);
                checkGameAccess(game, team, permission);
                game.CheckGameDeletable();
                _games.Remove(game.GameId);
                game.Dispose();
            }
        }

        public GameLiveInfo GetLiveInfo(int gameId, Team team, PermissionStrategy permission)
        {
            GameModel game;
            lock (_gameLock)
            {
                game = getGame(gameId);
                checkGameAccess(game, team, permission);
            }
           
            return game.GetLiveInfo();
        }

    }
}
