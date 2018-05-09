using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class FullToolbarPage : Page
    {
        public FullToolbarViewModel ViewModel { get; } = new FullToolbarViewModel();

        public FullToolbarPage()
        {
            InitializeComponent();
        }
    }
}
