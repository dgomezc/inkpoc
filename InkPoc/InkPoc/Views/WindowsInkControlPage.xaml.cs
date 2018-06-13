using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class WindowsInkControlPage : Page
    {
        public WindowsInkControlViewModel ViewModel { get; } = new WindowsInkControlViewModel();

        public WindowsInkControlPage()
        {
            InitializeComponent();
        }

        private void InkControl_OnCopy(object sender, EventArgs e)
        {
        }
    }
}
