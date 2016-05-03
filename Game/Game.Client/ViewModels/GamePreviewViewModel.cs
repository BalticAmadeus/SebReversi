using Game.AdminClient.AdminService;
using Game.AdminClient.Infrastructure;
using Game.AdminClient.Models;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GameLogic.UserManagement;

namespace Game.AdminClient.ViewModels
{
    public class GamePreviewViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly IAdministrationServiceGateway _administrationService;
        private readonly IMessageBoxDialogService _messageBoxDialogService;
        private readonly IConfirmationDialogService _confirmationDialogService;

        private int _gameId;

        public GamePreviewViewModel(
            IRegionManager regionManager,
            IAdministrationServiceGateway administrationService,
            IMessageBoxDialogService messageBoxDialogService,
            IConfirmationDialogService confirmationDialogService)
        {
            _regionManager = regionManager;
            _administrationService = administrationService;
            _messageBoxDialogService = messageBoxDialogService;
            _confirmationDialogService = confirmationDialogService;
        }

        #region Properties

        private readonly object _lock = new object();
        private bool _canEnter = true;

        private int _mapWidth;
        private const int RefreshTime = 300;
        private AutoRefreshOperation _autoRefreshOperation;

        public AutoRefreshOperation AutoRefreshOperation
        {
            get
            {
                return _autoRefreshOperation ?? (_autoRefreshOperation = new AutoRefreshOperation(async () =>
                {
                    lock (_lock)
                    {
                        if (!_canEnter)
                            return;

                        _canEnter = !_canEnter;
                    }

                    RequestCount++;

                    try
                    {
                        var games = await _administrationService.ListGamesAsync();
                        if (UserSettings.AutoOpen)
                        {
                            var lastActiveGameId = games
                                        .OrderByDescending(g => g.GameId)
                                        .Where(g => "Play".Equals(g.State, StringComparison.OrdinalIgnoreCase) || "Pause".Equals(g.State, StringComparison.OrdinalIgnoreCase))
                                        .Select(g => g.GameId)
                                        .FirstOrDefault();

                            if (lastActiveGameId > 0 && lastActiveGameId != _gameId)
                            {
                                _gameId = lastActiveGameId;
                            }
                        }

                        if (Game == null || Game.GameId != _gameId)
                        {
                            var game = await _administrationService.GetGameAsync(_gameId);
                         
                            Game = new GameViewModel
                            {
                                GameId = game.GameId,
                                Label = game.Label,
                                State = game.State,
                                PlayerCollection = new ObservableCollection<PlayerViewModel>(game.Players.Select(p => new PlayerViewModel
                                {
                                    GameId = p.GameId,
                                    Name = p.Name,
                                    PlayerId = p.PlayerId,
                                    Team = p.Team
                                }))
                            };

                            await ResetMap();

                            IsResumeEnabled = true;
                            IsPauseEnabled = false;
                        }

                        var turn = await _administrationService.GetNextTurnAsync(Game.GameId);

                        TurnQueueSize = turn.NumberOfQueuedTurns;

                        switch (turn.Game.State.ToUpper())
                        {
                            case "FINISH":
                                IsPauseEnabled = false;
                                IsResumeEnabled = false;
                                if (UserSettings.Role != TeamRole.Observer)
                                {
                                    //TODO Check
                                    AutoRefreshOperation.Pause();
                                }
                                break;
                            case "PAUSE":
                                IsPauseEnabled = false;
                                IsResumeEnabled = true;
                                break;
                            case "PLAY":
                                IsPauseEnabled = true;
                                IsResumeEnabled = false;
                                break;
                        }

                        if (turn.TurnNumber == -1 && turn.NumberOfQueuedTurns == -1)
                        {
                            await ResetMap();
                        }
                        else if (turn.TurnNumber != -1)
                        {
                            TurnNumber = turn.TurnNumber;

                            ChangeMap(turn.MapChanges);

                            var playerCollection = new ObservableCollection<PlayerStateViewModel>();

                            for (int i = 0; i < Game.PlayerCollection.Count; i++)
                            {
                                playerCollection.Add(new PlayerStateViewModel
                                {
                                    ColorId = i,
                                    Condition = turn.PlayerStates[i].Condition,
                                    Player = Game.PlayerCollection[i],
                                    Score = turn.PlayerStates[i].Score
                                });
                            }

                            PlayerCollection = playerCollection;
                        }

                        var liveInfo = await _administrationService.GetLiveInfo(Game.GameId);
                        for (int i = 0; i < PlayerCollection.Count; i++)
                        {
                            PlayerCollection[i].SlowTurn = liveInfo.GameLiveInfo.PlayerStates[i].SlowTurn;
                            PlayerCollection[i].Score = liveInfo.GameLiveInfo.PlayerStates[i].Score;
                        }

                    }
                    catch (Exception e)
                    {
                        AutoRefreshOperation.IsAutoRefreshEnabled = false;
                        _regionManager.RequestNavigate("MainRegion", new Uri("LobbyView", UriKind.Relative));
                        //_messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }

                    new Task(() =>
                    {
                        LastCallTime = _administrationService.LastCallTime;
                    }).Start();

                    lock (_lock)
                    {
                        _canEnter = !_canEnter;
                    }
                }, RefreshTime));
            }
        }

        private async void ChangeMap(IList<MapChange> mapChanges)
        {
            if (mapChanges.Any() == false)
                return;

            MapChange change = mapChanges[mapChanges.Count - 1];
            await AnimateTurn(change.Position.Y * _mapWidth + change.Position.X, change.Value);

            foreach (var mapChange in mapChanges)
            {
                int index = mapChange.Position.Y * _mapWidth + mapChange.Position.X;

                //Turn out of range map
                if(CellCollection.Count <= index)
                    continue;

                CellCollection[index].State = mapChange.Value;
            }
        }

        private async Task AnimateTurn(int index, string state)
        {
            //Turn out of range map
            if (CellCollection.Count <= index)
                return;

            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0)
                    CellCollection[index].State = state;
                else
                {
                    CellCollection[index].State = ".";
                }

                await Task.Delay(300);
            }
        }

        private async Task ResetMap()
        {
            MapResetCount++;

            var match = await _administrationService.GetMatchAsync(Game.GameId);
            _mapWidth = match.Map.Width;

            var cellViewModels = new List<CellViewModel>();

            for (int i = 0; i < match.Map.Rows.Count; i++)
            {
                for (int j = 0; j < match.Map.Rows[i].Length; j++)
                {
                    cellViewModels.Add(new CellViewModel
                    {
                        X = j,
                        Y = i,
                        State = Convert.ToString(match.Map.Rows[i][j]),
                        CellSize = 20,
                    });
                }
            }

            CellCollection = new ObservableCollection<CellViewModel>(cellViewModels);

            var playerCollection = new ObservableCollection<PlayerStateViewModel>();
            for (int i = 0; i < Game.PlayerCollection.Count; i++)
            {
                playerCollection.Add(new PlayerStateViewModel
                {
                    ColorId = i,
                    Condition = match.PlayerStates[i].Condition,
                    Player = Game.PlayerCollection[i]
                });
            }

            PlayerCollection = playerCollection;

            CanvasWidth = match.Map.Width * 20;
            CanvasHeight = match.Map.Height * 20;
        }

        private GameViewModel _game;
        public GameViewModel Game
        {
            get { return _game; }
            set { SetProperty(ref _game, value); }
        }

        private PlayerStateViewModel _selectedPlayer;
        public PlayerStateViewModel SelectedPlayer
        {
            get { return _selectedPlayer; }
            set { SetProperty(ref _selectedPlayer, value); }
        }

        private ObservableCollection<PlayerStateViewModel> _playerCollection;
        public ObservableCollection<PlayerStateViewModel> PlayerCollection
        {
            get { return _playerCollection; }
            set { SetProperty(ref _playerCollection, value); }
        }

        private ObservableCollection<CellViewModel> _cellCollection;
        public ObservableCollection<CellViewModel> CellCollection
        {
            get { return _cellCollection; }
            set { SetProperty(ref _cellCollection, value); }
        }

        private int _mapResetCount;
        public int MapResetCount
        {
            get { return _mapResetCount; }
            set { SetProperty(ref _mapResetCount, value); }
        }

        private int _turnNumber;
        public int TurnNumber
        {
            get { return _turnNumber; }
            set { SetProperty(ref _turnNumber, value); }
        }

        private double _turnQueueSize;
        public double TurnQueueSize
        {
            get { return _turnQueueSize; }
            set { SetProperty(ref _turnQueueSize, value); }
        }

        private double _requestCount;
        public double RequestCount
        {
            get { return _requestCount; }
            set { SetProperty(ref _requestCount, value); }
        }

        private bool _isPauseEnabled;
        public bool IsPauseEnabled
        {
            get { return _isPauseEnabled; }
            set { SetProperty(ref _isPauseEnabled, value); }
        }

        private bool _isResumeEnabled;
        public bool IsResumeEnabled
        {
            get { return _isResumeEnabled; }
            set { SetProperty(ref _isResumeEnabled, value); }
        }

        private double _canvasWidth;
        public double CanvasWidth
        {
            get { return _canvasWidth; }
            set { SetProperty(ref _canvasWidth, value); }
        }

        private double _canvasHeight;
        public double CanvasHeight
        {
            get { return _canvasHeight; }
            set { SetProperty(ref _canvasHeight, value); }
        }

        private double _lastCallTime;
        public double LastCallTime
        {
            get { return _lastCallTime; }
            set { SetProperty(ref _lastCallTime, value); }
        }

        public bool _isAutoOpenEnabled;
        public bool IsAutoOpenEnabled
        {
            get
            {
                _isAutoOpenEnabled = UserSettings.AutoOpen;
                return _isAutoOpenEnabled;
            }
            set
            {
                UserSettings.AutoOpen = value;
                SetProperty(ref _isAutoOpenEnabled, UserSettings.AutoOpen);
            }
        }

        #endregion

        #region Visibility

        public bool ShowResumeGame
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowPauseGame
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowDropPlayer
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowAutoOpen
        {
            get
            {
                return UserSettings.Role == TeamRole.Observer;
            }
        }

        #endregion

        #region Commands

        private AsyncDelegateCommandWrapper _closeCommand;
        public AsyncDelegateCommandWrapper CloseCommand
        {
            get
            {

                return _closeCommand ?? (_closeCommand = new AsyncDelegateCommandWrapper(() =>
                {
                    UserSettings.AutoOpen = false;
                    _regionManager.RequestNavigate("MainRegion", new Uri("LobbyView", UriKind.Relative));
                }));
            }
        }

        private AsyncDelegateCommandWrapper _pauseGameCommand;
        public AsyncDelegateCommandWrapper PauseGameCommand
        {
            get
            {
                return _pauseGameCommand ?? (_pauseGameCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (!IsPauseEnabled)
                        return;

                    IsPauseEnabled = false;
                    IsResumeEnabled = true;

                    try
                    {
                        await _administrationService.PauseGameAsync(Game.GameId);
                    }
                    catch (Exception e)
                    {
                        IsResumeEnabled = false;
                        IsPauseEnabled = true;

                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _resumeGameCommand;
        public AsyncDelegateCommandWrapper ResumeGameCommand
        {
            get
            {
                return _resumeGameCommand ?? (_resumeGameCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (!IsResumeEnabled)
                        return;

                    IsResumeEnabled = false;
                    IsPauseEnabled = true;

                    try
                    {
                        await _administrationService.ResumeGameAsync(Game.GameId);
                    }
                    catch (Exception e)
                    {
                        IsPauseEnabled = false;
                        IsResumeEnabled = true;

                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _dropPlayerCommand;
        public AsyncDelegateCommandWrapper DropPlayerCommand
        {
            get
            {
                return _dropPlayerCommand ?? (_dropPlayerCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (SelectedPlayer == null)
                        return;

                    string message = string.Format("Are you sure to drop player {0} {1}?", SelectedPlayer.Player.Team, SelectedPlayer.Player.Name);
                    bool result = _confirmationDialogService.OpenDialog("Drop Player", message);
                    if (!result)
                        return;

                    var selectedPlayer = SelectedPlayer;
                    SelectedPlayer = null;

                    try
                    {
                        await _administrationService.DropPlayer(Game.GameId, selectedPlayer.Player.PlayerId);
                    }
                    catch (Exception e)
                    {
                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _showInfoComamnd;
        public AsyncDelegateCommandWrapper ShowInfoCommand
        {
            get
            {
                return _showInfoComamnd ?? (_showInfoComamnd = new AsyncDelegateCommandWrapper(async () =>
                {
                    var response = await _administrationService.GetLiveInfo(Game.GameId);
                    string message;

                    using (var stringStream = new StringWriter())
                    {
                        var requestSerializer = new XmlSerializer(typeof(GetLiveInfoResp));
                        requestSerializer.Serialize(stringStream, response);
                        message = stringStream.ToString();
                    }

                    _messageBoxDialogService.OpenDialog(message, "Info");
                }));
            }
        }

        #endregion

        #region Navigation

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _gameId = (int)navigationContext.Parameters["SelectedGameId"];

            TurnQueueSize = 0;

            AutoRefreshOperation.Resume();

            TurnNumber = 0;
            MapResetCount = 0;

            RequestCount = 0;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            AutoRefreshOperation.Pause();
        }

        #endregion
    }
}
