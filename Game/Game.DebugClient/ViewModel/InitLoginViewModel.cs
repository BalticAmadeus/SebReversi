using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Prism.Commands;

namespace Game.DebugClient.ViewModel
{
    public class InitLoginViewModel : ServiceCallViewModel
    {
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        public InitLoginViewModel(
            ICommonDataManager commonDataManager,
            IServiceCallInvoker serviceCallInvoker,
            IMessageBoxDialogService messageBoxDialogService)
            : base(commonDataManager, serviceCallInvoker)
        {
            _messageBoxDialogService = messageBoxDialogService;
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
                            var req = new InitLoginReq
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

                            var initLoginResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<InitLoginReq, InitLoginResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/InitLogin", req);

                            if (!initLoginResp.IsOk())
                                return;

                            CommonDataManager.Challenge = initLoginResp.Challenge;
                        }
                        catch (Exception e)
                        {
                            _messageBoxDialogService.OpenDialog(e.Message, "Exception occurred");
                        }
                    });
                });
            }
        }

        public override string Title
        {
            get { return "Init Login"; }
        }
    }
}