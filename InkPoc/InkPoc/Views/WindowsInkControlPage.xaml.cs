using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace InkPoc.Views
{
    public sealed partial class WindowsInkControlPage : Page
    {
        public WindowsInkControlViewModel ViewModel { get; } = new WindowsInkControlViewModel();

        public WindowsInkControlPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
