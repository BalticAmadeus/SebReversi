using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Prism.Commands;

namespace Game.DebugClient.ViewModel
{
    public class GetPlayerViewViewModel : ServiceCallViewModel
    {
        private readonly IMapService _mapService;
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        private int _playerId;

        public GetPlayerViewViewModel(
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

                            var getPlayerViewResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<GetPlayerViewReq, GetPlayerViewResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/GetPlayerView", req);

                            CommonDataManager.SequenceNumber++;

                            if (!getPlayerViewResp.IsOk())
                                return;
                            
                            CommonDataManager.Turn = getPlayerViewResp.Turn;

                            var players =
                                getPlayerViewResp.PlayerStates.Select(
                                    (t, i) => new Player {Condition = t.Condition, Index = i}).ToList();
                            _mapService.UpdateMap(getPlayerViewResp.Map.Rows.ToArray(), players);
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

        public override string Title
        {
            get { return "Get Player View"; }
        }
    }
}