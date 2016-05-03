using System.Windows.Controls;
using Game.DebugClient.ViewModel;

namespace Game.DebugClient.Views
{
    public partial class InitLoginView : UserControl
    {
        public InitLoginView(InitLoginViewModel initLoginViewModel)
        {
            InitializeComponent();

            DataContext = initLoginViewModel;
        }
    }
}