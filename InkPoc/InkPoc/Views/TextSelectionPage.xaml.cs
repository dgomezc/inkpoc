using InkPoc.Helpers.Ink;
using InkPoc.Helpers.Ink.UndoRedo;
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

        private InkTransformManager transformManager;
        private InkUndoRedoManager undoRedoManager;
        private InkCopyPasteManager copyPasteManager;

        private InkLassoSelectionManager lassoSelectionManager;
        private InkNodeSelectionManager nodeSelectionManager;
        
        public TextSelectionPage()
        {
            InitializeComponent();

            strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            analyzer = new InkAsyncAnalyzer(inkCanvas, strokeService);
            transformManager = new InkTransformManager(drawingCanvas, strokeService);
            undoRedoManager = new InkUndoRedoManager(inkCanvas, analyzer, strokeService);
            copyPasteManager = new InkCopyPasteManager(strokeService);

            var selectionRectangleManager = new SelectionRectangleManager(inkCanvas, selectionCanvas, strokeService);
            lassoSelectionManager = new InkLassoSelectionManager(inkCanvas, selectionCanvas,strokeService, selectionRectangleManager);
            nodeSelectionManager = new InkNodeSelectionManager(inkCanvas, selectionCanvas, analyzer, strokeService, selectionRectangleManager);

            MouseInkButton.IsChecked = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            analyzer.ClearAnalysis();
            strokeService.ClearStrokes();            
            ClearSelection();
            undoRedoManager.Reset();
            drawingCanvas.Children.Clear();
        }

        private void SelectionButton_Checked(object sender, RoutedEventArgs e) => lassoSelectionManager.StartLassoSelectionConfig();

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e) => lassoSelectionManager.EndLassoSelectionConfig();

        private void MouseInkButton_Checked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

        private void MouseInkButton_Unchecked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            undoRedoManager.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            undoRedoManager.Redo();
        }

        private async void TransformTextAndShapes_Click(object sender, RoutedEventArgs e)
        {
            var result = await transformManager.TransformTextAndShapesAsync();

            if(result.TextAndShapes.Any())
            {
                ClearSelection();
                undoRedoManager.AddOperation(new TransformUndoRedoOperation(result, drawingCanvas, strokeService));
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            copyPasteManager.Cut();
            ClearSelection();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            copyPasteManager.Copy();
            ClearSelection();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            copyPasteManager.Paste();
            ClearSelection();
        }

        private void ClearSelection()
        {
            nodeSelectionManager.ClearSelection();
            lassoSelectionManager.ClearSelection();

        }
    }
}
