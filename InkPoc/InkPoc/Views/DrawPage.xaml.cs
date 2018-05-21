
using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class DrawPage : Page
    {
        public DrawViewModel ViewModel { get; } = new DrawViewModel();

        public DrawPage()
        {
            InitializeComponent();
        }
    }
}
