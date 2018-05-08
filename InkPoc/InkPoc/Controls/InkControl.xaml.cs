using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public sealed partial class InkControl : UserControl
    {
        public InkControl()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Pen |
                CoreInputDeviceTypes.Touch;
        }

        public bool ShowToolbar
        {
            get { return (bool)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }
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
        }
    }
}
