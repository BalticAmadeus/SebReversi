using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Game.DebugClient.Utilites;
using Prism.Commands;

namespace Game.DebugClient.ViewModel.Flows
{
    public class PlayerModeFlowViewModel : ServiceCallViewModel
    {
        private readonly IMapService _mapService;
        private readonly IMessageBoxDialogService _messageBoxDialogService;
        private ObservableCollection<CellViewModel> _cellCollection;
        private int _column;
        private bool _isExecuting;
        private ObservableCollection<PlayerViewModel> _playerCollection;
        private int _playerId;
        private int _row;
        private CellViewModel _selectedCell;
        private int _turn;
        private bool _yourTurn;
        private Timer _timerYourTurn;
        private string _startStopButtonState;

        public PlayerModeFlowViewModel(
            ICommonDataManager commonDataManager,
            IServiceCallInvoker serviceCallInvoker,
            IMapService mapService,
            IMessageBoxDialogService messageBoxDialogService) : base(commonDataManager, serviceCallInvoker)
        {
            _mapService = mapService;
            _messageBoxDialogService = messageBoxDialogService;
            _startStopButtonState = "Start";

            PlayerId = CommonDataManager.PlayerId;
            Turn = 0;

            CommonDataManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Turn")
                    Turn = CommonDataManager.Turn;

                if (args.PropertyName == "PlayerId")
                    PlayerId = CommonDataManager.PlayerId;
            };

            _mapService.MapChanged += (sender, args) =>
            {
                var cellViewModels = new List<CellViewModel>();

                for (var i = 0; i < _mapService.Map.Length; i++)
                {
                    for (var j = 0; j < _mapService.Map[i].Length; j++)
                    {
                        cellViewModels.Add(new CellViewModel
                        {
                            X = j,
                            Y = i,
                            State = Convert.ToString(_mapService.Map[i][j])
                        });
                    }
                }

                CellCollection = new ObservableCollection<CellViewModel>(cellViewModels);
                PlayerCollection =
                    new ObservableCollection<PlayerViewModel>(_mapService.Players.Select(p => new PlayerViewModel
                    {
                        Condition = p.Condition,
                        Index = p.Index
                    }));
            };

            //_mapService.CellChanged += (sender, args) =>
            //{
            //    var index = args.Y * _mapService.Map.First().Length + args.X;

            //    CellCollection[index].State = args.State;

            //    PlayerCollection = new ObservableCollection<PlayerViewModel>(_mapService.Players.Select(p => new PlayerViewModel
            //    {
            //        Condition = p.Condition,
            //        Index = p.Index
            //    }));
            //};

            if (_mapService.Map != null)
            {
                var cellViewModels = new List<CellViewModel>();

                for (var i = 0; i < _mapService.Map.Length; i++)
                {
                    for (var j = 0; j < _mapService.Map[i].Length; j++)
                    {
                        cellViewModels.Add(new CellViewModel
                        {
                            X = j,
                            Y = i,
                            State = Convert.ToString(_mapService.Map[i][j])
                        });
                    }
                }

                CellCollection = new ObservableCollection<CellViewModel>(cellViewModels);
                PlayerCollection = new ObservableCollection<PlayerViewModel>(_mapService.Players.Select(p => new PlayerViewModel
                {
                    Condition = p.Condition,
                    Index = p.Index
                }));
            }
            else
            {
                CellCollection = new ObservableCollection<CellViewModel>();
                PlayerCollection = new ObservableCollection<PlayerViewModel>();
            }

            _isExecuting = false;
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(async () => { await Task.Run(() => { _isExecuting = false; }); }); }
        }

        public int PlayerId
        {
            get { return _playerId; }
            set { SetProperty(ref _playerId, value); }
        }

        public int Turn
        {
            get { return _turn; }
            set { SetProperty(ref _turn, value); }
        }

        public int Column
        {
            get { return _column; }
            set { SetProperty(ref _column, value); }
        }

        public int Row
        {
            get { return _row; }
            set { SetProperty(ref _row, value); }
        }

        public ObservableCollection<PlayerViewModel> PlayerCollection
        {
            get { return _playerCollection; }
            set { SetProperty(ref _playerCollection, value); }
        }

        public bool YourTurn
        {
            get { return _yourTurn; }
            set { SetProperty(ref _yourTurn, value); }
        }

        public ObservableCollection<CellViewModel> CellCollection
        {
            get { return _cellCollection; }
            set { SetProperty(ref _cellCollection, value); }
        }

        public override string Title => "Player Mode";

        public CellViewModel SelectedCell
        {
            get { return _selectedCell; }
            set
            {
                SetProperty(ref _selectedCell, value);
                if (SelectedCell == null)
                    return;

                Row = SelectedCell.Y;
                Column = SelectedCell.X;

                Task.Run(async () =>
                {
                    try
                    {
                        var performMoveResp = await PerformMove(new EnPoint
                        {
                            Col = Column,
                            Row = Row
                        });

                        if (performMoveResp.IsOk() == false)
                            return;

                        _isExecuting = true;
                        var isTurnComplete = false;
                        while (isTurnComplete == false && _isExecuting)
                        {
                            var waitNextTurnResp = await WaitNextTurn(Turn == 0 ? 1 : Turn);

                            if (waitNextTurnResp.IsOk() == false)
                                return;

                            isTurnComplete = waitNextTurnResp.TurnComplete;
                            if (waitNextTurnResp.GameFinished)
                            {
                                _messageBoxDialogService.OpenDialog(waitNextTurnResp.FinishComment, waitNextTurnResp.FinishCondition);
                                _isExecuting = false;
                                return;
                            }
                        }

                        _isExecuting = false;
                        await GetMapChangesAndChangeMap();
                        YourTurn = false;
                    }
                    catch (Exception e)
                    {
                        _messageBoxDialogService.OpenDialog(e.Message, "Exception occurred");
                    }
                });
            }
        }

        public string StartStopButtonState
        {
            get { return _startStopButtonState; }
            set { SetProperty(ref _startStopButtonState, value); }
        }

        public ICommand StartStopCommand
        {
            get
            {
                return new RelayCommand(p => true, p => StartStopTimer());
            }
        }

        public ICommand PassCommand
        {
            get
            {
                return new RelayCommand(p => true, p => Pass());
            }
        }

        private async Task<PerformMoveResp> PerformMove(EnPoint turn, bool pass = false)
        {
            var performMoveReq = new PerformMoveReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                },

                PlayerId = PlayerId,
                Pass = pass,
                Turn = turn
            };

            var performMoveResp = await ServiceCallInvoker.InvokeAsync<PerformMoveReq, PerformMoveResp>(ServiceUrl.TrimEnd('/') + "/json/PerformMove", performMoveReq);
            CommonDataManager.SequenceNumber++;
            return performMoveResp;
        }

        private async void Pass()
        {
            var performMoveResp = await PerformMove(null, true);

            if (performMoveResp.IsOk())
            {
                _isExecuting = true;
                var isTurnComplete = false;
                while (isTurnComplete == false && _isExecuting)
                {
                    var waitNextTurnResp = await WaitNextTurn(Turn == 0 ? 1 : Turn);

                    if (waitNextTurnResp.IsOk() == false)
                        break;

                    isTurnComplete = waitNextTurnResp.TurnComplete;
                }

                _isExecuting = false;
                await GetMapChangesAndChangeMap();
            }

            YourTurn = false;
        }

        private async void StartStopTimer()
        {
            if (_timerYourTurn != null)
            {
                _timerYourTurn = null;
                StartStopButtonState = "Start";
            }
            else
            {
                _timerYourTurn = new Timer(WaitYourTurn, null, 1000, Timeout.Infinite);
                StartStopButtonState = "Stop";
            }
        }

        private async Task GetMapChangesAndChangeMap()
        {
            var req = new GetPlayerViewReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                },
                PlayerId = PlayerId
            };

            var getPlayerViewResp = await ServiceCallInvoker.InvokeAsync<GetPlayerViewReq, GetPlayerViewResp>(ServiceUrl.TrimEnd('/') + "/json/GetPlayerView", req);
            CommonDataManager.SequenceNumber++;

            if (getPlayerViewResp.IsOk())
            {
                CommonDataManager.Turn = getPlayerViewResp.Turn;

                var players = getPlayerViewResp.PlayerStates.Select((t, i) => new Player { Condition = "Opponent", Index = i }).ToList();
                players[getPlayerViewResp.Index].Condition = "You";
                _mapService.UpdateMap(getPlayerViewResp.Map.Rows.ToArray(), players);
            }

        }

        private async Task<WaitNextTurnResp> WaitNextTurn(int turn)
        {
            var waitNextTurnReq = new WaitNextTurnReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                },
                PlayerId = PlayerId,
                RefTurn = turn
            };

            var waitNextTurnResp = await ServiceCallInvoker.InvokeAsync<WaitNextTurnReq, WaitNextTurnResp>(ServiceUrl.TrimEnd('/') + "/json/WaitNextTurn", waitNextTurnReq);
            CommonDataManager.SequenceNumber++;
            return waitNextTurnResp;
        }

        private async Task Login()
        {
            CommonDataManager.SessionId = 0;
            CommonDataManager.SequenceNumber = 0;

            var initLoginReq = new InitLoginReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                }
            };

            var initLoginResp = await ServiceCallInvoker.InvokeAsync<InitLoginReq, InitLoginResp>(ServiceUrl.TrimEnd('/') + "/json/InitLogin", initLoginReq);

            if (initLoginResp.IsOk() == false)
                return;

            CommonDataManager.Challenge = initLoginResp.Challenge;
            var challenge = AuthCodeManager.GetAuthCode($"{AuthCodeManager.GetAuthCode($"{CommonDataManager.Challenge}{Password}")}{Password}");

            var completeLoginReq = new CompleteLoginReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                },
                ChallengeResponse = challenge
            };

            var completeLoginResp = await ServiceCallInvoker.InvokeAsync<CompleteLoginReq, CompleteLoginResp>(ServiceUrl.TrimEnd('/') + "/json/CompleteLogin", completeLoginReq);

            CommonDataManager.SequenceNumber++;

            if (completeLoginResp.IsOk() == false)
                return;

            CommonDataManager.SessionId = completeLoginResp.SessionId;
        }

        private async Task CreatePlayer()
        {
            var req = new CreatePlayerReq
            {
                Auth = new ReqAuth
                {
                    TeamName = TeamName,
                    AuthCode = AuthCode,
                    ClientName = Username,
                    SequenceNumber = SequenceNumber,
                    SessionId = SessionId
                }
            };

            var createPlayerResp = await ServiceCallInvoker.InvokeAsync<CreatePlayerReq, CreatePlayerResp>(ServiceUrl.TrimEnd('/') + "/json/CreatePlayer", req);
            CommonDataManager.SequenceNumber++;

            if (createPlayerResp.IsOk() == false)
                return;

            CommonDataManager.PlayerId = createPlayerResp.PlayerId;
        }

        private async Task WaitGameStart()
        {
            while (CommonDataManager.GameId < 1)
            {
                var req = new WaitGameStartReq
                {
                    Auth = new ReqAuth
                    {
                        TeamName = TeamName,
                        AuthCode = AuthCode,
                        ClientName = Username,
                        SequenceNumber = SequenceNumber,
                        SessionId = SessionId
                    },
                    PlayerId = PlayerId
                };

                var waitGameStartResp = await ServiceCallInvoker.InvokeAsync<WaitGameStartReq, WaitGameStartResp>(ServiceUrl.TrimEnd('/') + "/json/WaitGameStart", req);
                CommonDataManager.SequenceNumber++;
                if (waitGameStartResp.IsOk() == false)
                    return;
                if (waitGameStartResp.GameId > 0)
                    CommonDataManager.GameId = waitGameStartResp.GameId;
            }
        }

        private async void WaitYourTurn(object state)
        {
            if (CommonDataManager.SessionId == 0)
            {
                //Login
                await Login();
            }

            if (CommonDataManager.PlayerId == 0)
            {
                //Create player
                await CreatePlayer();
            }

            if (CommonDataManager.GameId == 0)
            {
                //Wait game
                await WaitGameStart();
                //Get map
                int turn = Turn;
                await GetMapChangesAndChangeMap(); //First map
                Turn = turn;
            }

            //Turn logic
            if (YourTurn == false && string.IsNullOrEmpty(AuthCode) == false)
            {
                var waitNextTurnResp = await WaitNextTurn(Turn);

                if (waitNextTurnResp.IsOk())
                {
                    YourTurn = waitNextTurnResp.YourTurn && waitNextTurnResp.TurnComplete && waitNextTurnResp.GameFinished == false;

                    if (waitNextTurnResp.YourTurn == false && waitNextTurnResp.TurnComplete)
                        Turn++;
                    if (YourTurn || waitNextTurnResp.TurnComplete && waitNextTurnResp.GameFinished == false)
                        await GetMapChangesAndChangeMap();

                    if (waitNextTurnResp.GameFinished && _timerYourTurn != null)
                    {
                        _messageBoxDialogService.OpenDialog(waitNextTurnResp.FinishComment, waitNextTurnResp.FinishCondition);
                        return;
                    }
                }
            }

            _timerYourTurn?.Change(1000, Timeout.Infinite);
        }
    }
}