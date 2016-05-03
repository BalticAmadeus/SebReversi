using Autofac;
using Game.DebugClient.Infrastructure;
using Game.DebugClient.Utilites;
using Game.DebugClient.ViewModel;
using Game.DebugClient.ViewModel.Flows;
using Game.DebugClient.Views;
using Game.DebugClient.Views.Flows;

namespace Game.DebugClient
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<SettingsManager>().As<ISettingsManager>().SingleInstance();
            builder.RegisterType<ServiceCallInvoker>().As<IServiceCallInvoker>().SingleInstance();
            builder.RegisterType<CommonDataManager>().As<ICommonDataManager>().SingleInstance();
            builder.RegisterType<MapService>().As<IMapService>().SingleInstance();
            builder.RegisterType<MessageBoxDialogService>().As<IMessageBoxDialogService>().InstancePerDependency();

            builder.RegisterType<LoggerViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<CommonDataViewModel>().AsSelf().SingleInstance();

            builder.RegisterType<CompleteLoginViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<CreatePlayerViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<GetPlayerViewViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<GetTurnResultViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<InitLoginViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LeaveGameViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PerformMoveViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<WaitGameStartViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<WaitNextTurnViewModel>().AsSelf().SingleInstance();

            builder.RegisterType<CompleteLoginFlowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<CreatePlayerFlowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<WaitGameStartFlowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PlayerModeFlowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<WaitNextTurnLoopViewModel>().AsSelf().SingleInstance();

            builder.RegisterType<MainView>().Named<object>("MainView").SingleInstance();
            builder.RegisterType<EmptyView>().Named<object>("EmptyView").SingleInstance();
            builder.RegisterType<CommonDataView>().Named<object>("CommonDataView").SingleInstance();
            builder.RegisterType<LoggerView>().Named<object>("LoggerView").SingleInstance();

            builder.RegisterType<CompleteLoginView>().Named<object>("CompleteLoginView").SingleInstance();
            builder.RegisterType<CreatePlayerView>().Named<object>("CreatePlayerView").SingleInstance();
            builder.RegisterType<GetPlayerViewView>().Named<object>("GetPlayerViewView").SingleInstance();
            builder.RegisterType<GetTurnResultView>().Named<object>("GetTurnResultView").SingleInstance();
            builder.RegisterType<InitLoginView>().Named<object>("InitLoginView").SingleInstance();
            builder.RegisterType<LeaveGameView>().Named<object>("LeaveGameView").SingleInstance();
            builder.RegisterType<PerformMoveView>().Named<object>("PerformMoveView").SingleInstance();
            builder.RegisterType<WaitGameStartView>().Named<object>("WaitGameStartView").SingleInstance();
            builder.RegisterType<WaitNextTurnView>().Named<object>("WaitNextTurnView").SingleInstance();

            builder.RegisterType<CompleteLoginFlowView>().Named<object>("CompleteLoginFlowView").SingleInstance();
            builder.RegisterType<CreatePlayerFlowView>().Named<object>("CreatePlayerFlowView").SingleInstance();
            builder.RegisterType<WaitGameStartFlowView>().Named<object>("WaitGameStartFlowView").SingleInstance();
            builder.RegisterType<PlayerModeFlowView>().Named<object>("PlayerModeFlowView").SingleInstance();
            builder.RegisterType<WaitNextTurnLoopView>().Named<object>("WaitNextTurnLoopView").SingleInstance();

            builder.RegisterType<JsonWebServiceClient>().As<IWebServiceClient>().SingleInstance();
        }
    }
}