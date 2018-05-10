using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using InkPoc.Helpers;
using System.Linq;
using InkPoc.Helpers.Ink;
using InkPoc.Services;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace InkPoc.Controls
{
    public sealed partial class InkControl : UserControl
    {
        public InkControl()
        {
            InitializeComponent();
            Loaded += InkControl_Loaded;           
        }

        private void InkControl_Loaded(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Pen |
                CoreInputDeviceTypes.Touch;

            UndoRedoManager = new InkUndoRedoManager(inkCanvas.InkPresenter);
            SelectionManager = new InkSelectionManager(inkCanvas.InkPresenter, selectionCanvas);
        }
        
        public InkUndoRedoManager UndoRedoManager { get; set; }

        public InkSelectionManager SelectionManager { get; set; }

        public InkStrokeContainer Strokes
        {
            get { return (InkStrokeContainer)GetValue(StrokesProperty); }
            set { SetValue(StrokesProperty, value); }
        }

        public static readonly DependencyProperty StrokesProperty =
            DependencyProperty.Register("Strokes", typeof(InkStrokeContainer),
                typeof(InkControl), new PropertyMetadata(null, OnStrokesChanged));

        private static void OnStrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is InkStrokeContainer strokes)
            {
                var control = d as InkControl;
                control.inkCanvas.InkPresenter.StrokeContainer = strokes;
            }
        }        

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            selectionCanvas.Children.Clear();
            UndoRedoManager.ClearUndoRedoStacks();
        }

        private void Undo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Undo();

        private void Redo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Redo();

        private async void openFile_Click(object sender, RoutedEventArgs e) => await InkService.LoadInkAsync(inkCanvas.InkPresenter.StrokeContainer);

        private async void SaveFile_Click(object sender, RoutedEventArgs e) => await InkService.SaveInkAsync(inkCanvas.InkPresenter.StrokeContainer);                
        
        private void Selection_Click(object sender, RoutedEventArgs e) => SelectionManager.StartSelection();

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            SelectionManager.ClearSelection();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.InkPresenter.StrokeContainer.CanPasteFromClipboard())
            {
                inkCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(20,20));
            }
        }
       
    }
}
