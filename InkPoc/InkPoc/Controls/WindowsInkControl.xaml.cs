using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using InkPoc.Services.Ink;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace InkPoc.Controls
{
    public sealed partial class WindowsInkControl : UserControl
    {
        private const bool DefaultEnableTouchValue = true;
        private const bool DefaultEnableMouseValue = true;

        private InkCopyPasteService _copyPasteService;
        private InkUndoRedoService _undoRedoService;
        private InkFileService _fileService;
        private InkZoomService _zoomService;
        private InkTransformService _transformService;
        private InkNodeSelectionService _nodeSelectionService;
        private InkAsyncAnalyzer _analyzer;

        private readonly InkStrokesService StrokeService;
        private readonly InkLassoSelectionService LassoSelectionService;
        private readonly InkPointerDeviceService PointerDeviceService;
        private readonly InkSelectionRectangleService SelectionRectangleService;
        private InkZoomService ZoomService => _zoomService ?? (_zoomService = new InkZoomService(canvasScroll));
        private InkCopyPasteService CopyPasteService => _copyPasteService ?? (_copyPasteService = new InkCopyPasteService(StrokeService));
        private InkUndoRedoService UndoRedoService => _undoRedoService ?? (_undoRedoService = new InkUndoRedoService(inkCanvas, StrokeService));

        private InkFileService FileService => _fileService ?? (_fileService = new InkFileService(inkCanvas, StrokeService));

        private InkTransformService TransformService => _transformService ?? (_transformService = new InkTransformService(drawingCanvas, StrokeService));

        private InkAsyncAnalyzer Analyzer => _analyzer ?? (_analyzer = new InkAsyncAnalyzer(inkCanvas, StrokeService));

        private InkNodeSelectionService NodeSelectionService => _nodeSelectionService ?? (_nodeSelectionService = new InkNodeSelectionService(inkCanvas, selectionCanvas, Analyzer, StrokeService, SelectionRectangleService));

        public event EventHandler OnCut;
        public event EventHandler OnCopy;
        public event EventHandler OnPaste;
        public event EventHandler OnFileOpened;
        public event EventHandler OnFileSaved;
        public event EventHandler OnImageExported;
        public event EventHandler OnTextAndShapesTransformed;
        public event EventHandler OnUndo;
        public event EventHandler OnRedo;
        public event EventHandler<float> OnZoomIn;
        public event EventHandler<float> OnZoomOut;

        #region Properties
        public ObservableCollection<InkOption> Options => (ObservableCollection<InkOption>)GetValue(OptionsProperty);

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(ObservableCollection<InkOption>), typeof(WindowsInkControl), new PropertyMetadata(null, OnOptionsPropertyChanged));

        public bool EnableLassoSelection
        {
            get => (bool)GetValue(EnableLassoSelectionProperty);
            set => SetValue(EnableLassoSelectionProperty, value);
        }

        public static readonly DependencyProperty EnableLassoSelectionProperty = DependencyProperty.Register(nameof(EnableLassoSelection), typeof(bool), typeof(WindowsInkControl), new PropertyMetadata(false, OnEnableLassoSelectionPropertyChanged));        

        public bool EnableTouch
        {
            get => (bool)GetValue(EnableTouchProperty);
            set => SetValue(EnableTouchProperty, value);
        }

        public static readonly DependencyProperty EnableTouchProperty = DependencyProperty.Register(nameof(EnableTouch), typeof(bool), typeof(WindowsInkControl), new PropertyMetadata(DefaultEnableTouchValue, OnEnableTouchPropertyChanged));

        public bool EnableMouse
        {
            get => (bool)GetValue(EnableMouseProperty);
            set => SetValue(EnableMouseProperty, value);
        }

        public static readonly DependencyProperty EnableMouseProperty = DependencyProperty.Register(nameof(EnableMouse), typeof(bool), typeof(WindowsInkControl), new PropertyMetadata(DefaultEnableMouseValue, OnEnableMousePropertyChanged));
        #endregion

        public WindowsInkControl()
        {
            this.InitializeComponent();
            SetValue(OptionsProperty, new ObservableCollection<InkOption>());
            StrokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            SelectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, StrokeService);
            LassoSelectionService = new InkLassoSelectionService(inkCanvas, selectionCanvas, StrokeService, SelectionRectangleService);
            PointerDeviceService = new InkPointerDeviceService(inkCanvas);
        }

        private static void OnOptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => (d as WindowsInkControl)?.UpdateOptionsProperty();
        private static void OnEnableLassoSelectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => (d as WindowsInkControl)?.UpdateEnableLassoSelectionProperty();
        private static void OnEnableTouchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => (d as WindowsInkControl)?.UpdateEnableTouchProperty();
        private static void OnEnableMousePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => (d as WindowsInkControl)?.UpdateEnableMouseProperty();

        private void UpdateOptionsProperty() => Options.CollectionChanged += OnOptionsCollectionChanged;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            inkCanvas.Width = inkCanvas.ActualWidth;
            inkCanvas.Height = inkCanvas.ActualHeight;

            selectionCanvas.Width = inkCanvas.Width;
            selectionCanvas.Height = inkCanvas.Height;
        }
        private void UpdateEnableLassoSelectionProperty()
        {
            if (EnableLassoSelection) LassoSelectionService.StartLassoSelectionConfig();
            else LassoSelectionService.EndLassoSelectionConfig();
        }
        private void UpdateEnableTouchProperty() => PointerDeviceService.EnableTouch = EnableTouch;
        private void UpdateEnableMouseProperty() => PointerDeviceService.EnableMouse = EnableMouse;
        private void OnOptionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var option in e.NewItems)
            {
                switch (option)
                {
                    case ZoomInkOption zoom:
                        EnableZoom(zoom);
                        break;
                    case CutCopyPasteInkOption cutCopyPaste:
                        EnableCutCopyPaste(cutCopyPaste);
                        break;
                    case UndoRedoInkOption undoRedo:
                        EnableUndoRedo(undoRedo);
                        break;
                    case FileImportExportInkOption fileImportExport:
                        EnableFileImportExport(fileImportExport);
                        break;
                    case TransformTextAndShapesInkOption transformTextAndShapes:
                        EnableTransformTextAndShapes(transformTextAndShapes);
                        break;
                    default:
                        break;
                }
            }
        }

        private void EnableZoom(ZoomInkOption zoom)
        {
            commandBar.PrimaryCommands.Add(new AppBarSeparator());

            var zoomInButton = zoom.ZoomInButton;
            zoomInButton.Click += (sender, e) => ZoomIn();
            commandBar.PrimaryCommands.Add(zoomInButton);

            var zoomOutButton = zoom.ZoomOutButton;
            zoomOutButton.Click += (sender, e) => ZoomOut();
            commandBar.PrimaryCommands.Add(zoomOutButton);
        }

        private void EnableCutCopyPaste(CutCopyPasteInkOption cutOption)
        {
            commandBar.PrimaryCommands.Add(new AppBarSeparator());

            var cutButton = cutOption.CutButton;
            cutButton.Click += (sender, e) => Cut();
            commandBar.PrimaryCommands.Add(cutButton);

            var copyButton = cutOption.CopyButton;
            copyButton.Click += (sender, e) => Copy();
            commandBar.PrimaryCommands.Add(copyButton);

            var pasteButton = cutOption.PasteButton;
            pasteButton.Click += (sender, e) => Paste();
            commandBar.PrimaryCommands.Add(pasteButton);
        }

        private void EnableUndoRedo(UndoRedoInkOption undoRedo)
        {
            commandBar.PrimaryCommands.Add(new AppBarSeparator());

            var undoButton = undoRedo.UndoButton;
            undoButton.Click += (sender, e) => Undo();
            commandBar.PrimaryCommands.Add(undoButton);

            var redoButton = undoRedo.RedoButton;
            redoButton.Click += (sender, e) => Redo();
            commandBar.PrimaryCommands.Add(redoButton);
        }

        private void EnableFileImportExport(FileImportExportInkOption fileImportExport)
        {
            commandBar.PrimaryCommands.Add(new AppBarSeparator());

            var openFileButton = fileImportExport.OpenFileButton;
            openFileButton.Click += async (sender, e) => await OpenFileAsync();
            commandBar.PrimaryCommands.Add(openFileButton);

            var saveFileButton = fileImportExport.SaveFileButton;
            saveFileButton.Click += async (sender, e) => await SaveFileAsync();
            commandBar.PrimaryCommands.Add(saveFileButton);

            var exportAsImageButton = fileImportExport.ExportAsImageButton;
            exportAsImageButton.Click += async (sender, e) => await ExportAsImageAsync();
            commandBar.PrimaryCommands.Add(exportAsImageButton);
        }

        private void EnableTransformTextAndShapes(TransformTextAndShapesInkOption transformTextAndShapes)
        {
            commandBar.PrimaryCommands.Add(new AppBarSeparator());

            var transformTextAndShapesButton = transformTextAndShapes.TransformTextAndShapesButton;
            transformTextAndShapesButton.Click += async (sender, e) => await TransformTextAndShapesAsync();
            commandBar.PrimaryCommands.Add(transformTextAndShapesButton);
        }

        public void ZoomIn()
        {
            var zoomFactor = ZoomService.ZoomIn();
            OnZoomIn?.Invoke(this, zoomFactor);
        }

        public void ZoomOut()
        {
            var zoomFactor = ZoomService.ZoomOut();
            OnZoomOut?.Invoke(this, zoomFactor);
        }

        public void Cut()
        {
            CopyPasteService.Cut();
            LassoSelectionService.ClearSelection();
            OnCut?.Invoke(this, EventArgs.Empty);
        }

        public void Copy()
        {
            CopyPasteService.Copy();
            LassoSelectionService.ClearSelection();
            OnCopy?.Invoke(this, EventArgs.Empty);
        }

        public void Paste()
        {
            CopyPasteService.Paste();
            LassoSelectionService.ClearSelection();
            OnPaste?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            LassoSelectionService.ClearSelection();
            UndoRedoService.Undo();
            OnUndo?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            LassoSelectionService.ClearSelection();
            UndoRedoService.Redo();
            OnRedo?.Invoke(this, EventArgs.Empty);
        }

        public async Task OpenFileAsync()
        {
            LassoSelectionService.ClearSelection();
            var fileLoaded = await FileService.LoadInkAsync();

            if (fileLoaded)
            {
                UndoRedoService.Reset();
                OnFileOpened?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task SaveFileAsync()
        {
            LassoSelectionService.ClearSelection();
            await FileService.SaveInkAsync();
            OnFileSaved?.Invoke(this, EventArgs.Empty);
        }

        public async Task ExportAsImageAsync()
        {
            LassoSelectionService.ClearSelection();
            await FileService.ExportToImageAsync();
            OnImageExported?.Invoke(this, EventArgs.Empty);
        }

        public async Task TransformTextAndShapesAsync()
        {
            var result = await TransformService.TransformTextAndShapesAsync();
            if (result.TextAndShapes.Any())
            {
                NodeSelectionService.ClearSelection();
                LassoSelectionService.ClearSelection();
                UndoRedoService.AddOperation(new TransformUndoRedoOperation(result, StrokeService));
                OnTextAndShapesTransformed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
