using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Game.DebugClient.ViewModel.Flows;
using Prism.Commands;

namespace Game.DebugClient.ViewModel
{
    public class PerformMoveViewModel : ServiceCallViewModel
    {
        private readonly IMapService _mapService;
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        private ObservableCollection<CellViewModel> _cellCollection;

        private int _column;

        private ObservableCollection<PlayerViewModel> _playerCollection;

        private int _playerId;

        private int _row;

        private CellViewModel _selectedCell;

        public PerformMoveViewModel(
            ICommonDataManager commonDataManager,
            IServiceCallInvoker serviceCallInvoker,
            IMapService mapService,
            IMessageBoxDialogService messageBoxDialogService)
            : base(commonDataManager, serviceCallInvoker)
        {
            CommonDataManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "PlayerId")
                    PlayerId = CommonDataManager.PlayerId;
            };

            _mapService = mapService;
            _messageBoxDialogService = messageBoxDialogService;
            PlayerId = CommonDataManager.PlayerId;

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
            };

            _mapService.CellChanged += (sender, args) =>
            {
                var index = args.Y*_mapService.Map.First().Length + args.X;

                CellCollection[index].State = args.State;
                PlayerCollection =
                    new ObservableCollection<PlayerViewModel>(_mapService.Players.Select(p => new PlayerViewModel
                    {
                        Condition = p.Condition,
                        Index = p.Index
                    }));
            };

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
                PlayerCollection =
                    new ObservableCollection<PlayerViewModel>(_mapService.Players.Select(p => new PlayerViewModel
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
        }

        public ICommand ExecuteCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var req = new PerformMoveReq
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
                                Turn = new EnPoint
                                {
                                    Col = Column,
                                    Row = Row
                                }
                            };

                            var performMoveResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<PerformMoveReq, PerformMoveResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/PerformMove", req);

                            CommonDataManager.SequenceNumber++;

                            if (!performMoveResp.IsOk())
                                return;
                        }
                        catch (Exception e)
                        {
                            _messageBoxDialogService.OpenDialog(e.Message, "Exception occurred");
                        }
                    });
                });
            }
        }

        public int PlayerId
        {
            get { return _playerId; }
            set { SetProperty(ref _playerId, value); }
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

        public CellViewModel SelectedCell
        {
            get { return _selectedCell; }
            set
            {
                SetProperty(ref _selectedCell, value);
                if (SelectedCell != null)
                {
                    Row = SelectedCell.Y;
                    Column = SelectedCell.X;
                }
            }
        }

        public ObservableCollection<PlayerViewModel> PlayerCollection
        {
            get { return _playerCollection; }
            set { SetProperty(ref _playerCollection, value); }
        }

        public ObservableCollection<CellViewModel> CellCollection
        {
            get { return _cellCollection; }
            set { SetProperty(ref _cellCollection, value); }
        }

        public override string Title
        {
            get { return "Perform Move"; }
        }
    }
}