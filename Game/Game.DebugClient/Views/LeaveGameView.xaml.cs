using System.Windows.Controls;
using Game.DebugClient.ViewModel;

namespace Game.DebugClient.Views
{
    public partial class LeaveGameView : UserControl
    {
        public LeaveGameView(LeaveGameViewModel leaveGameViewModel)
        {
            InitializeComponent();

            DataContext = leaveGameViewModel;
        }
    }
}