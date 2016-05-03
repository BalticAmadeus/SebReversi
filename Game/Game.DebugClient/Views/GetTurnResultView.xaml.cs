﻿using System.Windows.Controls;
using Game.DebugClient.ViewModel;

namespace Game.DebugClient.Views
{
    public partial class GetTurnResultView : UserControl
    {
        public GetTurnResultView(GetTurnResultViewModel getTurnResultViewModel)
        {
            InitializeComponent();

            DataContext = getTurnResultViewModel;
        }
    }
}