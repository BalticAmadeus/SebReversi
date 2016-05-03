using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Game.DebugClient.DataContracts;
using Game.DebugClient.Infrastructure;
using Game.DebugClient.Utilites;
using Prism.Commands;

namespace Game.DebugClient.ViewModel.Flows
{
    public class CompleteLoginFlowViewModel : ServiceCallViewModel
    {
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        private string _challenge;

        private string _challengeString;

        public CompleteLoginFlowViewModel(
            ICommonDataManager commonDataManager,
            IServiceCallInvoker serviceCallInvoker,
            IMessageBoxDialogService messageBoxDialogService)
            : base(commonDataManager, serviceCallInvoker)
        {
            _messageBoxDialogService = messageBoxDialogService;

            CommonDataManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Challenge")
                    ChallengeString = CommonDataManager.Challenge;
            };

            ChallengeString = CommonDataManager.Challenge;
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

                            var initLoginResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<InitLoginReq, InitLoginResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/InitLogin", initLoginReq);

                            if (!initLoginResp.IsOk())
                                return;

                            CommonDataManager.Challenge = initLoginResp.Challenge;

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
                                ChallengeResponse = Challenge
                            };

                            var completeLoginResp =
                                await
                                    ServiceCallInvoker.InvokeAsync<CompleteLoginReq, CompleteLoginResp>(
                                        ServiceUrl.TrimEnd('/') + "/json/CompleteLogin", completeLoginReq);

                            CommonDataManager.SequenceNumber++;

                            if (!completeLoginResp.IsOk())
                                return;
                            
                            CommonDataManager.SessionId = completeLoginResp.SessionId;
                        }
                        catch (Exception e)
                        {
                            _messageBoxDialogService.OpenDialog(e.Message, "Exception occurred");
                        }
                    });
                });
            }
        }

        public string Challenge
        {
            get { return _challenge; }
            set { SetProperty(ref _challenge, value); }
        }

        public string ChallengeString
        {
            get { return _challengeString; }
            set { SetProperty(ref _challengeString, value); }
        }

        public override string Title
        {
            get { return "Init Login & Complete Login"; }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Challenge")
                return;

            Challenge = AuthCodeManager.GetAuthCode(string.Format("{0}{1}",
                AuthCodeManager.GetAuthCode(string.Format("{0}{1}", ChallengeString, Password)), Password));

            base.OnPropertyChanged(sender, args);
        }
    }
}