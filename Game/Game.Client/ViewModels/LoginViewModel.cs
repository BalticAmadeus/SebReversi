using Game.AdminClient.Infrastructure;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows;
using GameLogic.UserManagement;

namespace Game.AdminClient.ViewModels
{
    public class LoginViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ISettingsManager _settingsManager;
        private readonly IAdministrationServiceGateway _administrationServiceGateway;
        private readonly IMessageBoxDialogService _messageBoxDialogService;

        public LoginViewModel(
            IRegionManager regionManager, 
            ISettingsManager settingsManager, 
            IAdministrationServiceGateway administrationServiceGateway,
            IMessageBoxDialogService messageBoxDialogService)
        {
            _regionManager = regionManager;
            _settingsManager = settingsManager;
            _administrationServiceGateway = administrationServiceGateway;
            _messageBoxDialogService = messageBoxDialogService;
        }

        #region Properties

        public string ServiceUrl
        {
            set { _settingsManager.ServiceUrl = value; }
            get { return _settingsManager.ServiceUrl; }
        }

        public string TeamName
        {
            set { _settingsManager.TeamName = value; }
            get { return _settingsManager.TeamName; }
        }

        public string Username
        {
            set { _settingsManager.Username = value; }
            get { return _settingsManager.Username; }
        }
        
        public string Password
        {
            set { _settingsManager.Password = value; }
            get { return _settingsManager.Password; }
        }

        #endregion

        #region Commands

        private AsyncDelegateCommandWrapper _confirmCommand;
        public AsyncDelegateCommandWrapper ConfirmCommand
        {
            get
            {
                return _confirmCommand ?? (_confirmCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    try
                    {
#if DEBUG
                        if (string.IsNullOrEmpty(ServiceUrl))
                        {
                            ServiceUrl = "http://localhost:60044/AdminService.svc";
                            TeamName = "Admin";
                            Username = "Admin";
                            Password = "GangnamStyle";
                        }
#endif
                        Validation.NotNullOrWhitespace(ServiceUrl, "Service url");
                        Validation.NotNullOrWhitespace(TeamName, "Team name");
                        Validation.NotNullOrWhitespace(Username, "Username");

                        if (!ServiceUrl.ToLower().EndsWith(".svc"))
                        {
                            ServiceUrl = ServiceUrl.TrimEnd('/') + "/AdminService.svc";
                        }

                        _administrationServiceGateway.ConnectionData = new ConnectionData
                        {
                            Password = Password,
                            TeamName = TeamName,
                            Url = ServiceUrl,
                            Username = Username,
                        };

                        UserSettings.Role = await _administrationServiceGateway.LoginAsync();
                        UserSettings.SetTitle();
                        UserSettings.AutoOpen = UserSettings.Role == TeamRole.Observer;

                        _regionManager.RequestNavigate("MainRegion", new Uri("LobbyView", UriKind.Relative));
                    }
                    catch (Exception e)
                    {
                        _messageBoxDialogService.ShowException(e);
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _exitCommand;
        public AsyncDelegateCommandWrapper ExitCommand
        {
            get
            {
                return _exitCommand ?? (_exitCommand = new AsyncDelegateCommandWrapper(() => Application.Current.Shutdown()));
            }
        }

        #endregion
    }

    public static class Validation
    {
        public static void NotNullOrWhitespace(string input, string label)
        {
            if (string.IsNullOrWhiteSpace(input))
                ThrowException($"Please enter {label}");
        }

        private static void ThrowException(string message)
        {
            throw new ValidationException(message);
        }
    }

    public class ValidationException : ApplicationException
    {
        public ValidationException(string message) : base(message)
        {
            // nothing more
        }
    }
}
