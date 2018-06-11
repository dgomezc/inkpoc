
using InkPoc.Services.Ink;
using InkPoc.Services.Ink.UndoRedo;
using InkPoc.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class DrawPage : Page
    {
        public DrawViewModel ViewModel { get; } = new DrawViewModel();

        public DrawPage()
        {
            InitializeComponent();
            Loaded += (s,e) => SetCanvasSize();

            var strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            var selectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, strokeService);
            
            ViewModel = new DrawViewModel(
                strokeService,
                new InkLassoSelectionService(inkCanvas, selectionCanvas, strokeService, selectionRectangleService),
                new InkPointerDeviceService(inkCanvas),
                new InkCopyPasteService(strokeService),
                new InkUndoRedoService(inkCanvas, strokeService),
                new InkFileService(inkCanvas, strokeService),
                new InkZoomService(canvasScroll));
        }

        private void SetCanvasSize()
        {
            inkCanvas.Width = inkCanvas.ActualWidth;
            inkCanvas.Height = inkCanvas.ActualHeight;

            selectionCanvas.Width = inkCanvas.Width;
            selectionCanvas.Height = inkCanvas.Height;
        }
    }
}
