using InkPoc.Services.Ink;
using InkPoc.Services.Ink.UndoRedo;
using InkPoc.Services.Ink;
using InkPoc.ViewModels;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class TextSelectionPage : Page
    {
        public TextSelectionViewModel ViewModel { get; } = new TextSelectionViewModel();

        private readonly InkStrokesService strokeService;
        private InkAsyncAnalyzer analyzer;

        private InkTransformService transformService;
        private InkUndoRedoService undoRedoService;
        private InkCopyPasteService copyPasteService;

        private InkLassoSelectionService lassoSelectionService;
        private InkNodeSelectionService nodeSelectionService;
        
        public TextSelectionPage()
        {
            InitializeComponent();

            strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            analyzer = new InkAsyncAnalyzer(inkCanvas, strokeService);
            transformService = new InkTransformService(drawingCanvas, strokeService);
            undoRedoService = new InkUndoRedoService(inkCanvas, strokeService);
            copyPasteService = new InkCopyPasteService(strokeService);

            var selectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, strokeService);
            lassoSelectionService = new InkLassoSelectionService(inkCanvas, selectionCanvas,strokeService, selectionRectangleService);
            nodeSelectionService = new InkNodeSelectionService(inkCanvas, selectionCanvas, analyzer, strokeService, selectionRectangleService);

            MouseInkButton.IsChecked = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            analyzer.ClearAnalysis();
            strokeService.ClearStrokes();
            ClearSelection();
            undoRedoService.Reset();
            drawingCanvas.Children.Clear();
        }

        private void SelectionButton_Checked(object sender, RoutedEventArgs e) => lassoSelectionService.StartLassoSelectionConfig();

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e) => lassoSelectionService.EndLassoSelectionConfig();

        private void MouseInkButton_Checked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

        private void MouseInkButton_Unchecked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            undoRedoService.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            undoRedoService.Redo();
        }

        private async void TransformTextAndShapes_Click(object sender, RoutedEventArgs e)
        {
            var result = await transformService.TransformTextAndShapesAsync();

            if(result.TextAndShapes.Any())
            {
                ClearSelection();
                undoRedoService.AddOperation(new TransformUndoRedoOperation(result, drawingCanvas, strokeService));
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            copyPasteService.Cut();
            ClearSelection();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            copyPasteService.Copy();
            ClearSelection();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            copyPasteService.Paste();
            ClearSelection();
        }

        private void ClearSelection()
        {
            nodeSelectionService.ClearSelection();
            lassoSelectionService.ClearSelection();

        }
    }
}
