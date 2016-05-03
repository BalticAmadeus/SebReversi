using Prism.Mvvm;

namespace Game.DebugClient.ViewModel
{
    public class CellViewModel : BindableBase
    {
        public const double CellSize = 20;

        private int _y;

        private string _state;

        private int _x;

        public int X
        {
            get { return _x; }
            set
            {
                SetProperty(ref _x, value);
                OnPropertyChanged(() => Left);
            }
        }

        public int Y
        {
            get { return _y; }
            set
            {
                SetProperty(ref _y, value);
                OnPropertyChanged(() => Top);
            }
        }

        public string State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public double Left => _x*CellSize;

        public double Top => _y*CellSize;
    }
}