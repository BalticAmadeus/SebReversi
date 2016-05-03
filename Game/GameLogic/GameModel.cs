using GameLogic.Reversi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameLogic
{
    public class GameModel : IDisposable
    {
        public int GameId { get; private set; }
        public Team Owner { get; private set; }

        private GameState _state;
        private readonly List<Player> _players;
        private readonly List<ObserverQueue> _observers;
        private MapData _map;
        private readonly GameProtocol _proto;

        private object _liveLock;
        private Dictionary<int, int> _indexes;
        private PlayerState[] _playerStates;
        private int _gameTurnEnded;
        private int _gameTurnStarted;
        private DateTime _turnStart;
        private DateTime _turnEnd;
        private readonly int _turnDuration;

        public GameModel(int gameId, Team owner)
        {
            GameId = gameId;
            Owner = owner;
            State = GameState.Setup;
            _players = new List<Player>();
            _observers = new List<ObserverQueue>();
            _proto = new GameProtocol(this);
            _turnDuration = Settings.DefaultGameTurnDurationMillis;
        }

        public GameState State
        {
            get
            {
                return _state;
            }

            private set
            {
                _state = value;
                _proto?.LogGameState(_state);
            }
        }

        public string Label
        {
            get
            {
                string label = $"Game #{GameId}: {Owner}";

                if (_players != null && _players.Any())
                {
                    label += " - ";
                    for (int i=0; i<_players.Count; i++)
                    {
                        label += $"{_players[i].Team?.Name} ({_players[i].Name})";
                        if (_players.Count > i + 1)
                            label += " vs ";
                    }
                }

                return label;
            }
        }

        public void Dispose()
        {
            _proto.Dispose();
        }

        private void CheckSetupState()
        {
            if (State != GameState.Setup)
                throw new ApplicationException("Game must be in SETUP state");
        }

        public void CheckRunState()
        {
            if (State == GameState.Setup)
                throw new ApplicationException("Game must be started");
        }

        public void CheckPlayState()
        {
            if (State != GameState.Play && State != GameState.Pause)
                throw new ApplicationException("Game must be playing");
        }

        public void CheckGameDeletable()
        {
            if (State == GameState.Setup)
                return;
            lock (_liveLock)
            {
                //if (State != GameState.Finish)
                //    throw new ApplicationException("Game must be finished");
                //if (_playerStates.Any(p => p.IsPresent)) // Should never happen but just as a precaution
                //    throw new ApplicationException("All players must leave");
            }
        }

        public void AddPlayer(Player player)
        {
            CheckSetupState();
            if (player.Game != null)
                throw new ApplicationException("Player already in a game");
            _players.Add(player);
            player.Game = this;
        }

        public void RemovePlayer(Player player)
        {
            CheckSetupState();
            if (player.Game != this)
                throw new ApplicationException("Player is not in this game");
            _players.Remove(player);
            player.Game = null;
        }

        public IEnumerable<Player> ListPlayers()
        {
            return _players;
        }

        public void SetMap(MapData mapData)
        {
            CheckSetupState();
            _map = mapData;
        }

        public void Start()
        {
            CheckSetupState();
            if (_players.Count != 2)
                throw new ApplicationException("Number of players in the game must be 2");
            if (_map == null)
                throw new ApplicationException("Map is not loaded");

            _indexes = _players.Select((p, i) => Tuple.Create(p.PlayerId, i)).ToDictionary(k => k.Item1, v => v.Item2);

            // Find all starting positions and clean the map
            Dictionary<int, Point> starts = new Dictionary<int, Point>();
            for (int row = 0; row < _map.Height; row++)
            {
                for (int col = 0; col < _map.Width; col++)
                {
                    switch (_map.Tiles[row, col])
                    {
                        case TileType.Player1:
                        case TileType.Player2:
                            int p = _map.Tiles[row, col] - TileType.Player1;
                            starts[p] = new Point(row, col);
                            break;
                        case TileType.Empty:
                        default:
                            break;
                    }
                }
            }

            // Setup player initial states
            _playerStates = new PlayerState[_players.Count];
            for (int p = 0; p < _playerStates.Length; p++)
            {
                Point startingPosition;
                if (!starts.TryGetValue(p, out startingPosition))
                    throw new ApplicationException(string.Format("The map does not specify starting position for player {0}", p));
                _playerStates[p] = new PlayerState
                {
                    Index = p,
                    PlayerId = _players[p].PlayerId,
                    Condition = PlayerCondition.Play,
                    IsPresent = true,
                    LastPosition = startingPosition,
                    TurnPosition = startingPosition,
                    TurnFinTime = default(DateTime),
                    PenaltyPoints = 0,
                    BonusPoints = 0,
                    OvertimeTurnMsec = 0,
                    OvertimeTurnTurn = -1,
                    PenaltyThresholdReachedTurn = -1
                };
                starts.Remove(p);
            }

            _gameTurnEnded = 0;
            _gameTurnStarted = 0;
            PrepareTurnTimings();
            _liveLock = new object();

            // Start paused
            State = GameState.Pause;
            _proto.LogGameStart(this);
        }

        private void PrepareTurnTimings()
        {
            _turnStart = DateTime.Now;
            _turnEnd = _turnStart.AddMilliseconds(_turnDuration);
        }

        private int FindPlayer(int playerId)
        {
            int index;
            if (!_indexes.TryGetValue(playerId, out index))
                throw new ApplicationException("Player is not in this game");
            return index;
        }

        private ObserverQueue FindObserver(int observerId)
        {
            ObserverQueue q = _observers.FirstOrDefault(p => p.Observer.ObserverId == observerId);
            if (q == null)
                throw new ApplicationException("This observer does not watch this game");
            return q;
        }

        public GameViewInfo GetGameView(int playerId)
        {
            lock (_liveLock)
            {
                var gv = new GameViewInfo();

                if (playerId > 0)
                {
                    int p = FindPlayer(playerId);
                    if (!_playerStates[p].IsActive || _playerStates[p].TurnCompleted >= _gameTurnStarted)
                        throw new WaitException();
                    gv.PlayerIndex = p;
                }

                gv.GameState = State;
                gv.Turn = _gameTurnStarted;
                gv.YourTurn = ((gv.Turn % (gv.PlayerIndex + 1)) == 0);
                gv.PlayerStates = _playerStates.Select(s => new PlayerStateInfo(s)).ToArray();
                gv.Map = (MapData)_map.Clone();
                return gv;
            }
        }

        public void PerformMove(int playerId, Point position, bool pass)
        {
            lock (_liveLock)
            {
                CheckPlayState();
                int p = FindPlayer(playerId);
                if (!_playerStates[p].IsActive)
                    throw new ApplicationException("You cannot make more moves");

                if (_playerStates[p].TurnCompleted >= _gameTurnStarted)
                    throw new WaitException();

                // Just record the move
                _playerStates[p].TurnPosition = position;
                _playerStates[p].Pass = pass;
                _proto.LogMove(p, playerId, position, pass);
            }
        }

        private bool PlayerExitTest(WaitTurnInfo wi)
        {
            int p = wi.PlayerIndex;
            if (!_playerStates[p].IsActive)
            {
                wi.TurnComplete = true;
                wi.GameFinished = true;
                wi.FinishCondition = _playerStates[p].Condition;
                wi.FinishComment = _playerStates[p].Comment;
                return true;
            }
            return false;
        }

        public WaitTurnInfo CompletePlayerTurn(int playerId, int refTurn)
        {
            lock (_liveLock)
            {
                WaitTurnInfo wi = new WaitTurnInfo();
                int player = FindPlayer(playerId);
                if (_playerStates[player].IsPresent == false)
                    throw new ApplicationException("Player was dropped from the game");
                wi.PlayerIndex = player;

                if (refTurn == 0)
                {
                    // Crash recovery logic
                    if (_playerStates[player].TurnCompleted < _gameTurnStarted)
                    {
                        wi.TurnComplete = true;
                        return wi;
                    }
                    else
                    {
                        wi.Turn = _playerStates[player].TurnCompleted;
                        return wi;
                    }
                }

                if (_playerStates[player].TurnCompleted == refTurn)
                {
                    wi.Turn = refTurn;
                    return wi;
                }

                if (PlayerExitTest(wi))
                    return wi;

                if (_playerStates[player].TurnCompleted != refTurn - 1)
                    throw new ApplicationException($"Player is confusing turns: completed={_playerStates[player].TurnCompleted} refTurn={refTurn}");
                if (refTurn > _gameTurnStarted)
                    throw new ApplicationException($"Player skipping ahead of game progress: gameTurnStarted={_gameTurnStarted} refTurn={refTurn}");

                _playerStates[player].TurnCompleted = _gameTurnStarted;
                _playerStates[player].TurnFinTime = DateTime.Now;

                #region  Penalty logic
                int totalMsec = (int)(_playerStates[player].TurnFinTime - _turnStart).TotalMilliseconds;
                if (totalMsec > 1000)
                    _playerStates[player].PenaltyPoints += (totalMsec - 1000) / 100;
                else
                    _playerStates[player].BonusPoints += (1000 - totalMsec) / 100;
                if (totalMsec > Settings.TurnResponseThresholdMsec)
                {
                    _playerStates[player].OvertimeTurnMsec = totalMsec;
                    _playerStates[player].OvertimeTurnTurn = _playerStates[player].TurnCompleted;
                }
                if (_playerStates[player].PenaltyPoints > Settings.PenaltyPointsThreshold && _playerStates[player].PenaltyThresholdReachedTurn < 0)
                    _playerStates[player].PenaltyThresholdReachedTurn = _playerStates[player].TurnCompleted;

                _proto.LogPlayerTurnComplete(_playerStates[player], _turnStart);
                #endregion

                CompleteTurn();

                if (PlayerExitTest(wi))
                    return wi;

                wi.Turn = refTurn;
                return wi;
            }
        }

        private bool StartNextTurnMaybe()
        {
            if (_gameTurnStarted > _gameTurnEnded)
                return true;
            if (State != GameState.Play)
                return false;
            if (DateTime.Now < _turnEnd)
                return false;
            _gameTurnStarted = _gameTurnEnded + 1;
            PrepareTurnTimings();
            Monitor.PulseAll(_liveLock);
            _proto.LogGameTurnStart(_gameTurnStarted);
            return true;
        }

        private bool IsNeedTurn(int playerIndex, int turn)
        {
            if (playerIndex == 0)
                return turn % 2 == 0;
            return turn % 2 != 0;
        }

        private void CompleteTurn()
        {
            if (StartNextTurnMaybe() == false)
                return;
            if (_playerStates.Any(t => t.IsActive && t.TurnCompleted < _gameTurnStarted))
                return;

            // Move all heads to new positions
            Point[] necks = _playerStates.Select(p => p.LastPosition).ToArray();

            var rules = new ReversiRules(_map);
            var mapChanges = new List<IMapPoint>();

            for (int p = 0; p < _playerStates.Length; p++)
            {
                if (_playerStates[p].IsActive == false) continue;

                Point oldTurnPos = _playerStates[p].LastPosition;
                Point newTurnPos = _playerStates[p].TurnPosition;

                int playerIndex = _playerStates[p].Index;
                int turn = _playerStates[p].TurnCompleted - 1;
                TileType playerType = (TileType)_playerStates[p].Index + 1;
                bool needTurn = IsNeedTurn(playerIndex, turn);

                bool good = true;

                string reason = "OK";
                do
                {
                    if (needTurn == false && newTurnPos.Equals(oldTurnPos) == false && _playerStates[p].Pass == false)
                    {
                        good = false;
                        reason = "Did move when was not your turn";
                        break;
                    }

                    if (needTurn)
                    {
                        if (_playerStates[p].Pass)
                        {
                            var move = rules.GetFirstLegalMove(playerType);
                            if (move != null)
                            {
                                good = false;
                                reason = string.Format("Did pass when was legal move e.g. {0}", move);
                                break;
                            }
                        }
                        else
                        {
                            if (newTurnPos.Equals(oldTurnPos) == true)
                            {
                                good = false;
                                reason = "Did not move when was your turn";
                                break;
                            }

                            if (!rules.IsLegalMove(newTurnPos, playerType))
                            {
                                good = false;
                                reason = string.Format("Illegal move to {0}", newTurnPos);
                                mapChanges.Add(new ReversiPoint(newTurnPos, TileType.IllegalMove));
                                break;
                            }
                        }
                    }
                } while (false);

                if (!good)
                {
                    _playerStates[p].Condition = PlayerCondition.Draw;
                    _playerStates[p].Comment = reason;
                    _proto.LogPlayerCondition(_playerStates[p]);
                    continue;
                }

                if (!_playerStates[p].Pass)
                {
                    _playerStates[p].LastPosition = newTurnPos;

                    if (needTurn)
                        mapChanges.AddRange(rules.GetMapChanges(newTurnPos, playerType));
                }
            }

            // Update map
            foreach (var mapChange in mapChanges)
            {
                _map.ChangeTile(mapChange.Point, mapChange.PointType);
            }

            // Check game finish condition
            var activePlayers = _playerStates.Where(p => p.IsActive);
            if (activePlayers.Count() > 1)
            {
                foreach (var p in _playerStates)
                {
                    p.Score = rules.GetScore((TileType)p.Index + 1);
                }

                if (_map.IsFilled)
                {
                    StopGame();
                }
                else
                {
                    var canMove = false;
                    // GameModel continues
                    foreach (var p in _playerStates)
                    {
                        canMove = canMove || (rules.GetFirstLegalMove((TileType)p.Index + 1) != null);
                        if (p.Condition == PlayerCondition.Draw)
                        {
                            p.Condition = PlayerCondition.Lost;
                            _proto.LogPlayerCondition(p);
                        }
                    }

                    if (!canMove)
                    {
                        StopGame();
                    }
                }
            }
            else
            {
                // GameModel finishes
                State = GameState.Finish;
                if (activePlayers.Count() == 1)
                {
                    // We have a winner
                    PlayerState winner = activePlayers.Single();
                    winner.Condition = PlayerCondition.Won;
                    winner.Comment = "Congratulations !";
                    _proto.LogPlayerCondition(winner);
                    foreach (PlayerState p in _playerStates)
                    {
                        if (p.Condition == PlayerCondition.Draw)
                        {
                            p.Condition = PlayerCondition.Lost;
                            _proto.LogPlayerCondition(p);
                        }
                    }
                }
            }

            // Complete the turn
            _gameTurnEnded = _gameTurnStarted;
            _proto.LogGameTurnEnd(_gameTurnEnded);

            // Notify observers
            ObservedTurnInfo ot = new ObservedTurnInfo
            {
                Turn = _gameTurnEnded,
                GameState = State,
                PlayerStates = _playerStates.Select(p => new PlayerStateInfo(p)).ToArray(),
                MapChanges = mapChanges.ToArray()
            };

            bool haveObservers = false;
            foreach (ObserverQueue queue in _observers)
            {
                if (queue.Push(ot))
                    haveObservers = true;
            }
            if (haveObservers)
                Monitor.PulseAll(_liveLock);
            // Continue
            StartNextTurnMaybe();
        }

        private void StopGame()
        {
            State = GameState.Finish;

            var maxScore = _playerStates.Max(p => p.Score);
            var condition = PlayerCondition.Draw;
            var winners = _playerStates.Where(p => p.Score == maxScore);
            if (winners.Count() == 1) condition = PlayerCondition.Won;
            foreach (var winner in winners)
            {
                winner.Condition = condition;
                winner.Comment = "Score: " + winner.Score;
                _proto.LogPlayerCondition(winner);
            }
            var losers = _playerStates.Where(p => p.Score < maxScore);
            foreach (var loser in losers)
            {
                loser.Condition = PlayerCondition.Lost;
                loser.Comment = "Score: " + loser.Score;
                _proto.LogPlayerCondition(loser);
            }
        }

        public void WaitNextTurn(WaitTurnInfo wi)
        {
            lock (_liveLock)
            {
                if (PlayerExitTest(wi))
                    return;
                if (_gameTurnStarted > wi.Turn)
                {
                    wi.TurnComplete = true;
                    return;
                }
                CheckRunState();
                DateTime dtNow = DateTime.Now;
                if (_turnEnd > dtNow)
                {
                    int waitMillis = (int)(_turnEnd - dtNow).TotalMilliseconds;
                    if (waitMillis > Settings.NextTurnPollTimeoutMillis)
                        waitMillis = Settings.NextTurnPollTimeoutMillis;
                    else if (waitMillis < Settings.MinimumSleepMillis)
                        waitMillis = Settings.MinimumSleepMillis;
                    Monitor.Wait(_liveLock, waitMillis);
                }
                else
                {
                    Monitor.Wait(_liveLock, Settings.NextTurnPollTimeoutMillis);
                }

                StartNextTurnMaybe();

                if (PlayerExitTest(wi))
                    return;
                if (_gameTurnStarted > wi.Turn)
                    wi.TurnComplete = true;
            }
        }

        public GameResultInfo GetTurnResult()
        {
            lock (_liveLock)
            {
                return new GameResultInfo
                {
                    Turn = _gameTurnStarted,
                    GameState = State,
                    PlayerStates = _playerStates.Select(p => new PlayerStateInfo(p)).ToArray()
                };
            }
        }

        public void DropPlayer(Player player, string reason)
        {
            lock (_liveLock)
            {
                CheckRunState();
                int p = FindPlayer(player.PlayerId);
                if (!_playerStates[p].IsPresent)
                    return; // It can't happen but we should be ok anyway
                if (_playerStates[p].IsActive)
                {
                    _playerStates[p].Condition = PlayerCondition.Lost;
                    _playerStates[p].Comment = string.Format("Dropped from the game ({0})", reason);
                    _proto.LogPlayerCondition(_playerStates[p]);
                    CompleteTurn();
                }
                _playerStates[p].IsPresent = false;
                _proto.LogPlayerDrop(p, player.PlayerId, reason);
                player.Game = null;
            }
        }

        public void Pause()
        {
            lock (_liveLock)
            {
                CheckPlayState();
                State = GameState.Pause;
            }
        }

        public void Resume()
        {
            lock (_liveLock)
            {
                CheckPlayState();
                State = GameState.Play;
                Monitor.PulseAll(_liveLock);
            }
        }

        private void RemoveObserver(int observerId)
        {
            _observers.RemoveAll(q => q.Observer.ObserverId == observerId);
        }

        private GameViewInfo AddObserver(Observer observer)
        {
            _observers.Add(new ObserverQueue(observer));
            return GetGameView(-1);
        }

        public GameViewInfo StartObserving(Observer observer)
        {
            lock (_liveLock)
            {
                CheckRunState();
                RemoveObserver(observer.ObserverId);
                return AddObserver(observer);
            }
        }

        public ObservedGameInfo ObserveNextTurn(Observer observer)
        {
            lock (_liveLock)
            {
                CheckRunState();
                ObserverQueue q = FindObserver(observer.ObserverId);
                ObservedTurnInfo ot = q.Pop();
                if (ot == null && q.IsLive)
                {
                    Monitor.Wait(_liveLock, Settings.ObserverPollTimeoutMillis);
                    ot = q.Pop();
                }
                ObservedGameInfo gi = new ObservedGameInfo
                {
                    GameId = GameId,
                    GameState = State,
                    QueuedTurns = (q.IsLive) ? q.Count : -1,
                    TurnInfo = ot
                };
                return gi;
            }
        }

        public GameLiveInfo GetLiveInfo()
        {
            lock (_liveLock)
            {
                CheckRunState();
                GameLiveInfo gi = new GameLiveInfo
                {
                    GameState = State,
                    Turn = _gameTurnStarted,
                    TurnStartTime = _turnStart
                };

                gi.PlayerStates = _playerStates.Select(p => new PlayerLiveInfo
                {
                    PlayerId = p.PlayerId,
                    Team = _players[p.Index].Team,
                    Name = _players[p.Index].Name,
                    Condition = p.Condition,
                    Comment = p.Comment,
                    TurnCompleted = p.TurnCompleted,
                    TurnFinTime = p.TurnFinTime,
                    PenaltyPoints = p.PenaltyPoints,
                    BonusPoints = p.BonusPoints,
                    OvertimeTurnMsec = p.OvertimeTurnMsec,
                    OvertimeTurnTurn = p.OvertimeTurnTurn,
                    PenaltyThresholdReachedTurn = p.PenaltyThresholdReachedTurn,
                    Score = p.Score

                }).ToArray();

                //CheckSlowTurn(gi);

                return gi;
            }
        }

        //private void CheckSlowTurn(GameLiveInfo gi)
        //{
        //    DateTime startTurn = gi.TurnStartTime;
        //    int turn = gi.Turn;

        //    foreach (var playerLiveInfo in gi.PlayerStates)
        //    {
        //        if (gi.GameState != GameState.Play)
        //            playerLiveInfo.SlowTurn = false;

        //        else if (playerLiveInfo.TurnCompleted == turn)
        //            playerLiveInfo.SlowTurn = playerLiveInfo.TurnFinTime > startTurn.AddSeconds(Settings.SlowTurnIntervalSeconds);
        //        else
        //        {
        //            playerLiveInfo.SlowTurn = DateTime.Now.AddSeconds(-1 * Settings.SlowTurnIntervalSeconds) > startTurn;
        //        }
        //    }
        //}
    }

    public enum GameState
    {
        Setup, Play, Pause, Finish
    }

    public class PlayerState
    {
        public int Index;
        public int PlayerId;
        public PlayerCondition Condition;
        public bool IsActive => Condition == PlayerCondition.Play;
        public bool IsPresent;
        public string Comment;
        public Point LastPosition;
        public int TurnCompleted;
        public Point TurnPosition;
        public bool Pass;

        public DateTime TurnFinTime;
        public int PenaltyPoints;
        public int BonusPoints;
        public int OvertimeTurnMsec;
        public int OvertimeTurnTurn;
        public int PenaltyThresholdReachedTurn;

        public int Score;
    }

    public enum PlayerCondition
    {
        Play, Won, Lost, Draw
    }

}
