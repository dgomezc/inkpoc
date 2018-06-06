using InkPoc.Helpers.Ink;
using InkPoc.Helpers.Ink.UndoRedo;
using InkPoc.Services.Ink;
using InkPoc.ViewModels;
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

        private InkSelectionAndMoveManager selectionManager;
        private InkConversionManager conversionManager;
        private InkUndoRedoManager undoRedoManager;

        public TextSelectionPage()
        {
            InitializeComponent();

            strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            analyzer = new InkAsyncAnalyzer(strokeService);
            selectionManager = new InkSelectionAndMoveManager(inkCanvas, selectionCanvas, analyzer, strokeService);
            conversionManager = new InkConversionManager(drawingCanvas, strokeService);
            undoRedoManager = new InkUndoRedoManager(inkCanvas, analyzer, strokeService);

            MouseInkButton.IsChecked = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            analyzer.ClearAnalysis();
            strokeService.ClearStrokes();            
            selectionManager.ClearSelection();
            undoRedoManager.Reset();
            drawingCanvas.Children.Clear();
        }

        private void SelectionButton_Checked(object sender, RoutedEventArgs e) => selectionManager.StartLassoSelectionConfig();

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e) => selectionManager.EndLassoSelectionConfig();

        private void MouseInkButton_Checked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

        private void MouseInkButton_Unchecked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            selectionManager.ClearSelection();
            undoRedoManager.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            selectionManager.ClearSelection();
            undoRedoManager.Redo();
        }

        private async void ConvertTextAndShapes_Click(object sender, RoutedEventArgs e)
        {
            await conversionManager.ConvertTextAndShapesAsync();
            selectionManager.ClearSelection();
        }
    }
}
