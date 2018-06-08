using System;
using InkPoc.Services.Ink;
using InkPoc.Services.Ink.UndoRedo;
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
            Loaded += (s, e) => SetCanvasSize();

            var strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            var analyzer = new InkAsyncAnalyzer(inkCanvas, strokeService);
            var selectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, strokeService);

            ViewModel = new SmartCanvasViewModel(
                strokeService,
                new InkLassoSelectionService(inkCanvas, selectionCanvas, strokeService, selectionRectangleService),
                new InkNodeSelectionService(inkCanvas, selectionCanvas, analyzer, strokeService, selectionRectangleService),
                new InkPointerDeviceService(inkCanvas),
                new InkUndoRedoService(inkCanvas, strokeService),
                new InkTransformService(drawingCanvas,strokeService),
                new InkFileService(inkCanvas, strokeService));
        }
        private void SetCanvasSize()
        {
            inkCanvas.Width = inkCanvas.ActualWidth;
            inkCanvas.Height = inkCanvas.ActualHeight;

            selectionCanvas.Width = inkCanvas.Width;
            selectionCanvas.Height = inkCanvas.Height;

            drawingCanvas.Width = inkCanvas.Width;
            drawingCanvas.Height = inkCanvas.Height;
        }
    }
}
