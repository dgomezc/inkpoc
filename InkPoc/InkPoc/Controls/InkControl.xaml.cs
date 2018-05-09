using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using InkPoc.Helpers;
using System.Linq;
using InkPoc.Helpers.Ink;

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

        public bool ShowToolbar
        {
            get { return (bool)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

        public InkUndoRedoManager UndoRedoManager { get; set; }

        public InkStrokeContainer Strokes
        {
            get { return (InkStrokeContainer)GetValue(StrokesProperty); }
            set { SetValue(StrokesProperty, value); }
        }

        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register("ShowToolbar", typeof(bool),
                typeof(InkControl), new PropertyMetadata(true, OnShowToolbarChanged));

        public static readonly DependencyProperty StrokesProperty =
            DependencyProperty.Register("Strokes",
                typeof(InkStrokeContainer), typeof(InkControl), new PropertyMetadata(null, OnStrokesChanged));

        private static void OnShowToolbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InkControl;
            var toolbar = control?.toolbar;

            if (toolbar != null)
            {
                toolbar.Visibility = (bool)e.NewValue
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private static void OnStrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is InkStrokeContainer strokes)
            {
                var control = d as InkControl;
                control.inkCanvas.InkPresenter.StrokeContainer = strokes;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            canvas.Children.Clear();
            UndoRedoManager.ClearUndoRedoStacks();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Undo();

        private void RedoButton_Click(object sender, RoutedEventArgs e) => UndoRedoManager.Redo();

    }
}
