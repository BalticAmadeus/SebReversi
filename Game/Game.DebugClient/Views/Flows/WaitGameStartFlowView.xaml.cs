using System.Windows.Controls;
using Game.DebugClient.ViewModel.Flows;

namespace Game.DebugClient.Views.Flows
{
    public partial class WaitGameStartFlowView : UserControl
    {
        public WaitGameStartFlowView(WaitGameStartFlowViewModel waitGameStartFlowViewModel)
        {
            InitializeComponent();

            DataContext = waitGameStartFlowViewModel;
        }
    }
}