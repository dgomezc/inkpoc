using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class BasicDrawPage : Page
    {
        public BasicDrawViewModel ViewModel { get; } = new BasicDrawViewModel();

        public BasicDrawPage()
        {
            InitializeComponent();
        }
    }
}
