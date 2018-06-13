using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private readonly InkStrokesService _strokeService;
        private InkLassoSelectionService _lassoSelectionService;
        private InkPointerDeviceService _pointerDeviceService;
        private InkCopyPasteService _copyPasteService;
        private InkUndoRedoService _undoRedoService;
        private InkFileService _fileService;
        private InkZoomService _zoomService;

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
            _strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
            var selectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, _strokeService);
            _lassoSelectionService = new InkLassoSelectionService(inkCanvas, selectionCanvas, _strokeService, selectionRectangleService);
            _pointerDeviceService = new InkPointerDeviceService(inkCanvas);
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
            if (EnableLassoSelection) _lassoSelectionService.StartLassoSelectionConfig();
            else _lassoSelectionService.EndLassoSelectionConfig();
        }
        private void UpdateEnableTouchProperty() => _pointerDeviceService.EnableTouch = EnableTouch;
        private void UpdateEnableMouseProperty() => _pointerDeviceService.EnableMouse = EnableMouse;
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
                    default:
                        break;
                }
            }
        }

        private void EnableZoom(ZoomInkOption zoom)
        {
            _zoomService = new InkZoomService(canvasScroll);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            var zoomInButton = zoom.ZoomInButton;
            zoomInButton.Click += OnZoomInButtonClick;
            commandBar.PrimaryCommands.Add(zoomInButton);
            var zoomOutButton = zoom.ZoomOutButton;
            zoomOutButton.Click += OnZoomOutButtonClick;
            commandBar.PrimaryCommands.Add(zoomOutButton);
        }

        private void EnableCutCopyPaste(CutCopyPasteInkOption cutOption)
        {
            _copyPasteService = new InkCopyPasteService(_strokeService);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            var cutButton = cutOption.CutButton;
            cutButton.Click += OnCutButtonClick;
            commandBar.PrimaryCommands.Add(cutButton);
            var copyButton = cutOption.CopyButton;
            copyButton.Click += OnCopyButtonClick;
            commandBar.PrimaryCommands.Add(copyButton);
            var pasteButton = cutOption.PasteButton;
            pasteButton.Click += OnPasteButtonClick;
            commandBar.PrimaryCommands.Add(pasteButton);
        }

        private void EnableUndoRedo(UndoRedoInkOption undoRedo)
        {
            _undoRedoService = new InkUndoRedoService(inkCanvas, _strokeService);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            var undoButton = undoRedo.UndoButton;
            undoButton.Click += OnUndoButtonClick;
            commandBar.PrimaryCommands.Add(undoButton);
            var redoButton = undoRedo.RedoButton;
            redoButton.Click += OnRedoButtonClick;
            commandBar.PrimaryCommands.Add(redoButton);
        }

        private void EnableFileImportExport(FileImportExportInkOption fileImportExport)
        {
            _fileService = new InkFileService(inkCanvas, _strokeService);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            var openFileButton = fileImportExport.OpenFileButton;
            openFileButton.Click += OnOpenFileButtonClick;
            commandBar.PrimaryCommands.Add(openFileButton);
            var saveFileButton = fileImportExport.SaveFileButton;
            saveFileButton.Click += OnSaveFileButtonClick;
            commandBar.PrimaryCommands.Add(saveFileButton);
            var exportAsImageButton = fileImportExport.ExportAsImageButton;
            exportAsImageButton.Click += OnExportAsImageButtonClick;
            commandBar.PrimaryCommands.Add(exportAsImageButton);
        }

        private void OnZoomInButtonClick(object sender, RoutedEventArgs e) => _zoomService.ZoomIn();

        private void OnZoomOutButtonClick(object sender, RoutedEventArgs e) => _zoomService.ZoomOut();

        private void OnCutButtonClick(object sender, RoutedEventArgs e)
        {
            _copyPasteService.Cut();
            _lassoSelectionService?.ClearSelection();
        }

        private void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            _copyPasteService.Copy();
            _lassoSelectionService?.ClearSelection();
        }

        private void OnPasteButtonClick(object sender, RoutedEventArgs e)
        {
            _copyPasteService.Paste();
            _lassoSelectionService?.ClearSelection();
        }

        private void OnUndoButtonClick(object sender, RoutedEventArgs e)
        {
            _lassoSelectionService?.ClearSelection();
            _undoRedoService.Undo();
        }

        private void OnRedoButtonClick(object sender, RoutedEventArgs e)
        {
            _lassoSelectionService?.ClearSelection();
            _undoRedoService.Redo();
        }

        private async void OnOpenFileButtonClick(object sender, RoutedEventArgs e)
        {
            _lassoSelectionService?.ClearSelection();
            var fileLoaded = await _fileService.LoadInkAsync();

            if (fileLoaded)
            {
                _undoRedoService?.Reset();
            }
        }

        private async void OnSaveFileButtonClick(object sender, RoutedEventArgs e)
        {
            _lassoSelectionService?.ClearSelection();
            await _fileService.SaveInkAsync();
        }

        private async void OnExportAsImageButtonClick(object sender, RoutedEventArgs e)
        {
            _lassoSelectionService.ClearSelection();
            await _fileService.ExportToImageAsync();
        }
    }
}
