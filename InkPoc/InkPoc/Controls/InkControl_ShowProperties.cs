using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public sealed partial class InkControl : UserControl
    {
        public Visibility ShowToolbar
        {
            get { return (Visibility)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register("ShowToolbar",
                typeof(Visibility), typeof(InkControl), new PropertyMetadata(Visibility.Visible));

        public Visibility ShowSelectionTool
        {
            get { return (Visibility)GetValue(ShowSelectionToolProperty); }
            set { SetValue(ShowSelectionToolProperty, value); }
        }

        public static readonly DependencyProperty ShowSelectionToolProperty =
            DependencyProperty.Register("ShowSelectionTool",
                typeof(Visibility), typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ShowCopyPaste
        {
            get { return (Visibility)GetValue(ShowCopyPasteProperty); }
            set { SetValue(ShowCopyPasteProperty, value); }
        }

        public static readonly DependencyProperty ShowCopyPasteProperty =
            DependencyProperty.Register("ShowCopyPaste", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ShowUndoRedo
        {
            get { return (Visibility)GetValue(ShowUndoRedoProperty); }
            set { SetValue(ShowUndoRedoProperty, value); }
        }

        public static readonly DependencyProperty ShowUndoRedoProperty =
            DependencyProperty.Register("ShowUndoRedo", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ShowZoom
        {
            get { return (Visibility)GetValue(ShowZoomProperty); }
            set { SetValue(ShowZoomProperty, value); }
        }

        public static readonly DependencyProperty ShowZoomProperty =
            DependencyProperty.Register("ShowZoom", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ShowOpenSaveFile
        {
            get { return (Visibility)GetValue(ShowOpenSaveFileProperty); }
            set { SetValue(ShowOpenSaveFileProperty, value); }
        }

        public static readonly DependencyProperty ShowOpenSaveFileProperty =
            DependencyProperty.Register("ShowOpenSaveFile", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ShowClearAll
        {
            get { return (Visibility)GetValue(ShowClearAllProperty); }
            set { SetValue(ShowClearAllProperty, value); }
        }

        public static readonly DependencyProperty ShowClearAllProperty =
            DependencyProperty.Register("ShowClearAll", typeof(Visibility),
                typeof(InkControl), new PropertyMetadata(Visibility.Collapsed));

    }
}
