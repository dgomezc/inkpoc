﻿using InkPoc.Services.Ink;
using InkPoc.Services.Ink.UndoRedo;
using InkPoc.Services;
using InkPoc.Services.Ink;
using System;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace InkPoc.Controls
{
    public sealed partial class InkControl : UserControl
    {
        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register("ShowToolbar", typeof(bool), typeof(InkControl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowSelectionToolProperty =
            DependencyProperty.Register("ShowSelectionTool", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowCopyPasteProperty =
            DependencyProperty.Register("ShowCopyPaste", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowUndoRedoProperty =
            DependencyProperty.Register("ShowUndoRedo", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowZoomProperty =
            DependencyProperty.Register("ShowZoom", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowOpenSaveFileProperty =
            DependencyProperty.Register("ShowOpenSaveFile", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowClearAllProperty =
            DependencyProperty.Register("ShowClearAll", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowExportFileProperty =
            DependencyProperty.Register("ShowExportFile", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowRecognizeProperty =
            DependencyProperty.Register("ShowRecognize", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowEnableTouchInkingProperty =
            DependencyProperty.Register("ShowEnableTouchInking", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowEnableMouseInkingProperty =
                    DependencyProperty.Register("ShowEnableMouseInking", typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public static readonly DependencyProperty StrokesProperty =
            DependencyProperty.Register("Strokes", typeof(InkStrokeContainer), typeof(InkControl), new PropertyMetadata(null, OnStrokesChanged));

        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.Register("ImageFile", typeof(StorageFile), typeof(InkControl), new PropertyMetadata(null, OnImageFileChanged));

        public static readonly DependencyProperty CanvasSizeProperty =
            DependencyProperty.Register("CanvasSize", typeof(Size), typeof(InkControl), new PropertyMetadata(null, OnCanvasSizeChanged));

        public static readonly DependencyProperty EnableTouchProperty =
            DependencyProperty.Register("EnableTouch", typeof(bool), typeof(InkControl), new PropertyMetadata(true, OnEnableTouchChanged));

        public static readonly DependencyProperty EnableMouseProperty =
                    DependencyProperty.Register("EnableMouse", typeof(bool), typeof(InkControl), new PropertyMetadata(true, OnEnableMouseChanged));

        public InkControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);
                var analyzer = new InkAsyncAnalyzer(inkCanvas, strokeService);

                PointerDeviceService = new InkPointerDeviceService(inkCanvas);
                UndoRedoService = new InkUndoRedoService(inkCanvas, strokeService);
                var selectionRectangleService = new InkSelectionRectangleService(inkCanvas, selectionCanvas, strokeService);
                LassoSelectionService = new InkLassoSelectionService(inkCanvas, selectionCanvas, strokeService, selectionRectangleService);
                NodeSelectionService = new InkNodeSelectionService(inkCanvas, selectionCanvas, analyzer, strokeService, selectionRectangleService);
                TransformService = new InkTransformService(drawingCanvas, strokeService);
                CopyPasteService = new InkCopyPasteService(strokeService);
                FileService = new InkFileService(inkCanvas, strokeService);

                if (CanvasSize.Height == 0 && CanvasSize.Width == 0)
                {
                    CanvasSize = new Size(inkCanvas.ActualWidth, inkCanvas.ActualHeight);
                }
            };
        }

        public InkUndoRedoService UndoRedoService { get; set; }
        public InkPointerDeviceService PointerDeviceService { get; set; }

        public InkTransformService TransformService { get; set; }

        public InkCopyPasteService CopyPasteService { get; set; }

        public InkFileService FileService { get; set; }

        public InkLassoSelectionService LassoSelectionService { get; set; }

        public InkNodeSelectionService NodeSelectionService { get; set; }

        public bool ShowToolbar
        {
            get { return (bool)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

        public bool ShowSelectionTool
        {
            get { return (bool)GetValue(ShowSelectionToolProperty); }
            set { SetValue(ShowSelectionToolProperty, value); }
        }

        public bool ShowCopyPaste
        {
            get { return (bool)GetValue(ShowCopyPasteProperty); }
            set { SetValue(ShowCopyPasteProperty, value); }
        }

        public bool ShowUndoRedo
        {
            get { return (bool)GetValue(ShowUndoRedoProperty); }
            set { SetValue(ShowUndoRedoProperty, value); }
        }

        public bool ShowZoom
        {
            get { return (bool)GetValue(ShowZoomProperty); }
            set { SetValue(ShowZoomProperty, value); }
        }

        public bool ShowOpenSaveFile
        {
            get { return (bool)GetValue(ShowOpenSaveFileProperty); }
            set { SetValue(ShowOpenSaveFileProperty, value); }
        }

        public bool ShowClearAll
        {
            get { return (bool)GetValue(ShowClearAllProperty); }
            set { SetValue(ShowClearAllProperty, value); }
        }

        public bool ShowExportFile
        {
            get { return (bool)GetValue(ShowExportFileProperty); }
            set { SetValue(ShowExportFileProperty, value); }
        }

        public bool ShowRecognize
        {
            get { return (bool)GetValue(ShowRecognizeProperty); }
            set { SetValue(ShowRecognizeProperty, value); }
        }

        public bool ShowEnableTouchInking
        {
            get { return (bool)GetValue(ShowEnableTouchInkingProperty); }
            set { SetValue(ShowEnableTouchInkingProperty, value); }
        }
        public bool ShowEnableMouseInking
        {
            get { return (bool)GetValue(ShowEnableMouseInkingProperty); }
            set { SetValue(ShowEnableMouseInkingProperty, value); }
        }

        public InkStrokeContainer Strokes
        {
            get { return (InkStrokeContainer)GetValue(StrokesProperty); }
            set { SetValue(StrokesProperty, value); }
        }

        public StorageFile ImageFile
        {
            get { return (StorageFile)GetValue(ImageFileProperty); }
            set { SetValue(ImageFileProperty, value); }
        }

        public Size CanvasSize
        {
            get { return (Size)GetValue(CanvasSizeProperty); }
            set { SetValue(CanvasSizeProperty, value); }
        }

        public bool EnableTouch
        {
            get { return (bool)GetValue(EnableTouchProperty); }
            set { SetValue(EnableTouchProperty, value); }
        }

        public bool EnableMouse
        {
            get { return (bool)GetValue(EnableMouseProperty); }
            set { SetValue(EnableMouseProperty, value); }
        }

        private static void OnStrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is InkStrokeContainer strokes)
            {
                var control = d as InkControl;
                control.inkCanvas.InkPresenter.StrokeContainer = strokes;
            }
        }

        private static async void OnImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var file = e.NewValue as StorageFile;

            if (control != null && file != null)
            {
                control.Clear();
                var bitmapImage = new BitmapImage();

                using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(fileStream);
                }

                control.CanvasSize = new Size(bitmapImage.PixelWidth, bitmapImage.PixelHeight);

                Image image = new Image();
                image.Source = bitmapImage;
                control.drawingCanvas.Children.Add(image);
            }
        }

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

        private static void OnEnableTouchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var enableTouchValue = (bool)e.NewValue;

            control.PointerDeviceService.EnableTouch = enableTouchValue;
        }

        private static void OnEnableMouseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var enableMouseValue = (bool)e.NewValue;

            control.PointerDeviceService.EnableMouse = enableMouseValue;
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => Clear();

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            UndoRedoService.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            UndoRedoService.Redo();
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor + 0.2f);

        private void ZoomOut_Click(object sender, RoutedEventArgs e) => canvasScroll.ChangeView(canvasScroll.HorizontalOffset, canvasScroll.VerticalOffset, canvasScroll.ZoomFactor - 0.2f);

        private async void openFile_Click(object sender, RoutedEventArgs e) => await FileService.LoadInkAsync();

        private async void SaveFile_Click(object sender, RoutedEventArgs e) => await FileService.SaveInkAsync();

        private void SelectionButton_Checked(object sender, RoutedEventArgs e) => LassoSelectionService.StartLassoSelectionConfig();

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e) => LassoSelectionService.EndLassoSelectionConfig();

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            CopyPasteService.Cut();
            ClearSelection();
        }
        
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            CopyPasteService.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {            
            CopyPasteService.Paste();
            ClearSelection();
        }
        
        private async void Export_Click(object sender, RoutedEventArgs e) => await FileService.ExportToImageAsync(ImageFile);

        private async void TransformTextAndShapes_Click(object sender, RoutedEventArgs e) => await TransformService.TransformTextAndShapesAsync();
        
        private void Clear()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            selectionCanvas.Children.Clear();
            drawingCanvas.Children.Clear();
            UndoRedoService.Reset();
        }

        private void ClearSelection()
        {
            NodeSelectionService.ClearSelection();
            LassoSelectionService.ClearSelection();
        }
    }
}
