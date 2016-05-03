using System.Windows.Controls;
using Game.DebugClient.ViewModel;

namespace Game.DebugClient.Views
{
    public partial class WaitGameStartView : UserControl
    {
        public WaitGameStartView(WaitGameStartViewModel waitGameStartViewModel)
        {
            InitializeComponent();

            DataContext = waitGameStartViewModel;
        }
    }
}