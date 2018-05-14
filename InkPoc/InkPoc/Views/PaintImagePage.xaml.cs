using System;
using System.Threading.Tasks;
using InkPoc.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace InkPoc.Views
{
    public sealed partial class PaintImagePage : Page
    {
        public PaintImageViewModel ViewModel { get; } = new PaintImageViewModel();

        public PaintImagePage()
        {
            InitializeComponent();
        }
    }
}
