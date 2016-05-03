using System.Windows.Controls;
using Game.DebugClient.ViewModel;

namespace Game.DebugClient.Views
{
    public partial class CompleteLoginView : UserControl
    {
        public CompleteLoginView(CompleteLoginViewModel initLoginViewModel)
        {
            InitializeComponent();

            DataContext = initLoginViewModel;
        }
    }
}