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
using System;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

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

            UndoRedoManager = new InkSimpleUndoRedoManager(inkCanvas.InkPresenter);
            SelectionManager = new InkSelectionManager(inkCanvas.InkPresenter, selectionCanvas);
            RecognizeManager = new InkRecognizeManager(inkCanvas.InkPresenter, drawingCanvas);

            if (CanvasSize.Height == 0 && CanvasSize.Width == 0)
            {
                CanvasSize = new Size(inkCanvas.ActualWidth, inkCanvas.ActualHeight);
            }
        }
        
        public InkSimpleUndoRedoManager UndoRedoManager { get; set; }

        public InkSelectionManager SelectionManager { get; set; }

        public InkRecognizeManager RecognizeManager { get; set; }

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
        
        public StorageFile ImageFile
        {
            get { return (StorageFile)GetValue(ImageFileProperty); }
            set { SetValue(ImageFileProperty, value); }
        }

        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.Register("ImageFile", typeof(StorageFile),
                typeof(InkControl), new PropertyMetadata(null, OnImageFileChanged));

        private static async void OnImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var file = e.NewValue as StorageFile;

            if (control != null && file != null)
            {
                control.Clear();

                using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);
                    control.imageCanvas.Source = bitmapImage;
                }
            }
        }

        public Size CanvasSize
        {
            get { return (Size)GetValue(CanvasSizeProperty); }
            set { SetValue(CanvasSizeProperty, value); }
        }

        public static readonly DependencyProperty CanvasSizeProperty =
            DependencyProperty.Register("CanvasSize", typeof(Size),
                typeof(InkControl), new PropertyMetadata(null, OnCanvasSizeChanged));

        private static void OnCanvasSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var newCanvasSize = (Size)e.NewValue;

            control.inkCanvas.Width = newCanvasSize.Width;
            control.inkCanvas.Height = newCanvasSize.Height;

            control.selectionCanvas.Width = newCanvasSize.Width;
            control.selectionCanvas.Height = newCanvasSize.Height;

            control.drawingCanvas.Width = newCanvasSize.Width;
            control.drawingCanvas.Height = newCanvasSize.Height;
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => Clear();

        private void Clear()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            selectionCanvas.Children.Clear();
            drawingCanvas.Children.Clear();
            UndoRedoManager.Reset();
            RecognizeManager.ClearAnalyzer();
            imageCanvas.Source = null;
        }

        private void Undo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Undo();

        private void Redo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Redo();

        private void ZoomIn_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor + 0.2f);

        private void ZoomOut_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor - 0.2f);

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

        private async void Export_Click(object sender, RoutedEventArgs e) => await InkService.ExportToImageAsync(inkCanvas.InkPresenter.StrokeContainer, CanvasSize, ImageFile);

        private void ImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => CanvasSize = e.NewSize;

        private async void recognize_Click(object sender, RoutedEventArgs e) => await RecognizeManager.AnalyzeStrokesAsync();
    }
}
