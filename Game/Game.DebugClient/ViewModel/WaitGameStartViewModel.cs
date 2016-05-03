using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Prism.Commands;

namespace Game.DebugClient.ViewModel
{
    public class WaitGameStartViewModel : ServiceCallViewModel
    {
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        private int _playerId;

        public WaitGameStartViewModel(
            ICommonDataManager commonDataManager,
            IServiceCallInvoker serviceCallInvoker,
            IMessageBoxDialogService messageBoxDialogService)
            : base(commonDataManager, serviceCallInvoker)
        {
            _messageBoxDialogService = messageBoxDialogService;
            CommonDataManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "PlayerId")
                    PlayerId = CommonDataManager.PlayerId;
            };

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

                            var waitGameStartResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<WaitGameStartReq, WaitGameStartResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/WaitGameStart", req);
                            CommonDataManager.SequenceNumber++;

                            if (!waitGameStartResp.IsOk())
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

        public override string Title
        {
            get { return "Wait Game Start"; }
        }
    }
}