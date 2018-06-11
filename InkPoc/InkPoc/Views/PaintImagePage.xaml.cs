using InkPoc.Services.Ink;
using InkPoc.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class PaintImagePage : Page
    {
        public PaintImageViewModel ViewModel { get; } = new PaintImageViewModel();

        public PaintImagePage()
        {
            InitializeComponent();
            Loaded += (s, e) => SetCanvasSize();

            var strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);

            ViewModel = new PaintImageViewModel(
                strokeService,
                new InkPointerDeviceService(inkCanvas),
                new InkFileService(inkCanvas, strokeService));
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor + 0.2f);

        private void ZoomOut_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor - 0.2f);

        private void SetCanvasSize()
        {
            inkCanvas.Width = inkCanvas.ActualWidth;
            inkCanvas.Height = inkCanvas.ActualHeight;
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0)
            {
                inkCanvas.Width = e.NewSize.Width;
            }

            if (e.NewSize.Height > 0)
            {
                inkCanvas.Height = e.NewSize.Height;
            }
        }
    }
}
