using System.Windows.Controls;
using Game.DebugClient.ViewModel.Flows;

namespace Game.DebugClient.Views.Flows
{
    public partial class CompleteLoginFlowView : UserControl
    {
        public CompleteLoginFlowView(CompleteLoginFlowViewModel completeLoginFlowViewModel)
        {
            InitializeComponent();

            DataContext = completeLoginFlowViewModel;
        }
    }
}