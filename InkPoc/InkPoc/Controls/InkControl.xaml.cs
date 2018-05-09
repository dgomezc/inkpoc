using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using InkPoc.Helpers;
using System.Linq;
using InkPoc.Helpers.Ink;
using InkPoc.Services;

namespace InkPoc.Controls
{
    public sealed partial class InkControl : UserControl
    {
        public InkControl()
        {
            InitializeComponent();
            Loaded += InkControl_Loaded;

            inkCanvas.InkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Pen |
                CoreInputDeviceTypes.Touch;
        }

        private void InkControl_Loaded(object sender, RoutedEventArgs e)
        {
            UndoRedoManager = new InkUndoRedoManager(inkCanvas.InkPresenter);
        }
        
        public InkUndoRedoManager UndoRedoManager { get; set; }

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
        
        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register("ShowToolbar",
                typeof(Visibility), typeof(InkControl), new PropertyMetadata(Visibility.Visible));
        
        public Visibility ShowLoadFile
        {
            get { return (Visibility)GetValue(ShowLoadFileProperty); }
            set { SetValue(ShowLoadFileProperty, value); }
        }

        public static readonly DependencyProperty ShowLoadFileProperty =
            DependencyProperty.Register("ShowLoadFile", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));
        
        public Visibility ShowSaveFile
        {
            get { return (Visibility)GetValue(ShowSaveFileProperty); }
            set { SetValue(ShowSaveFileProperty, value); }
        }

        public static readonly DependencyProperty ShowSaveFileProperty =
            DependencyProperty.Register("ShowSaveFile", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));
        
        public Visibility ShowUndoRedo
        {
            get { return (Visibility)GetValue(ShowUndoRedoProperty); }
            set { SetValue(ShowUndoRedoProperty, value); }
        }

        public static readonly DependencyProperty ShowUndoRedoProperty =
            DependencyProperty.Register("ShowUndoRedo", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));
        
        public Visibility ShowClearAll
        {
            get { return (Visibility)GetValue(ShowClearAllProperty); }
            set { SetValue(ShowClearAllProperty, value); }
        }

        public static readonly DependencyProperty ShowClearAllProperty =
            DependencyProperty.Register("ShowClearAll", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            canvas.Children.Clear();
            UndoRedoManager.ClearUndoRedoStacks();
        }

        private void Undo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Undo();

        private void Redo_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Redo();

        private async void openFile_Click(object sender, RoutedEventArgs e) => await InkService.LoadFileAsync(inkCanvas.InkPresenter.StrokeContainer);

        private async void SaveFile_Click(object sender, RoutedEventArgs e) => await InkService.SaveFileAsync(inkCanvas.InkPresenter.StrokeContainer);
                
        public Visibility ShowToolbar
        {
            get { return (Visibility)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

    }
}
