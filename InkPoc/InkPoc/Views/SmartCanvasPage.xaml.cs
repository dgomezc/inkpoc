using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class SmartCanvasPage : Page
    {
        public SmartCanvasViewModel ViewModel { get; } = new SmartCanvasViewModel();

        public SmartCanvasPage()
        {
            InitializeComponent();
        }
    }
}
