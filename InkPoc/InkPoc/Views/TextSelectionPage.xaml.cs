using System;
using System.Threading.Tasks;
using InkPoc.Helpers.Ink;
using InkPoc.Helpers.Ink.UndoRedo;
using InkPoc.ViewModels;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Views
{
    public sealed partial class TextSelectionPage : Page
    {
        public TextSelectionViewModel ViewModel { get; } = new TextSelectionViewModel();

        private InkAsyncAnalyzer analyzer;
        private InkSelectionAndMoveManager selectionManager;
        private InkUndoRedoWithAnalyzerManager undoRedoManager;

        public TextSelectionPage()
        {
            InitializeComponent();
            analyzer = new InkAsyncAnalyzer(inkCanvas.InkPresenter.StrokeContainer);
            selectionManager = new InkSelectionAndMoveManager(inkCanvas, selectionCanvas, analyzer);
            undoRedoManager = new InkUndoRedoWithAnalyzerManager(inkCanvas, analyzer);

            MouseInkButton.IsChecked = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            analyzer.StopTimer();

            inkCanvas.InkPresenter.StrokeContainer.Clear();
            analyzer.InkAnalyzer.ClearDataForAllStrokes();
            selectionManager.ClearSelection();
            undoRedoManager.Reset();
        }

        private void SelectionButton_Checked(object sender, RoutedEventArgs e) => selectionManager.StartLassoSelectionConfig();

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e) => selectionManager.EndLassoSelectionConfig();

        private void MouseInkButton_Checked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

        private void MouseInkButton_Unchecked(object sender, RoutedEventArgs e) => inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

        private void Undo_Click(object sender, RoutedEventArgs e) => undoRedoManager.Undo();

        private void Redo_Click(object sender, RoutedEventArgs e) => undoRedoManager.Redo();
    }
}
