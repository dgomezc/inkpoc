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
        public bool ShowToolbar
        {
            get { return (bool)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register("ShowToolbar",
                typeof(bool), typeof(InkControl), new PropertyMetadata(true));

        public bool ShowSelectionTool
        {
            get { return (bool)GetValue(ShowSelectionToolProperty); }
            set { SetValue(ShowSelectionToolProperty, value); }
        }

        public static readonly DependencyProperty ShowSelectionToolProperty =
            DependencyProperty.Register("ShowSelectionTool",
                typeof(bool), typeof(InkControl), new PropertyMetadata(false));

        public bool ShowCopyPaste
        {
            get { return (bool)GetValue(ShowCopyPasteProperty); }
            set { SetValue(ShowCopyPasteProperty, value); }
        }

        public static readonly DependencyProperty ShowCopyPasteProperty =
            DependencyProperty.Register("ShowCopyPaste", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowUndoRedo
        {
            get { return (bool)GetValue(ShowUndoRedoProperty); }
            set { SetValue(ShowUndoRedoProperty, value); }
        }

        public static readonly DependencyProperty ShowUndoRedoProperty =
            DependencyProperty.Register("ShowUndoRedo", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowZoom
        {
            get { return (bool)GetValue(ShowZoomProperty); }
            set { SetValue(ShowZoomProperty, value); }
        }

        public static readonly DependencyProperty ShowZoomProperty =
            DependencyProperty.Register("ShowZoom", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowOpenSaveFile
        {
            get { return (bool)GetValue(ShowOpenSaveFileProperty); }
            set { SetValue(ShowOpenSaveFileProperty, value); }
        }

        public static readonly DependencyProperty ShowOpenSaveFileProperty =
            DependencyProperty.Register("ShowOpenSaveFile", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowClearAll
        {
            get { return (bool)GetValue(ShowClearAllProperty); }
            set { SetValue(ShowClearAllProperty, value); }
        }

        public static readonly DependencyProperty ShowClearAllProperty =
            DependencyProperty.Register("ShowClearAll", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowExportFile
        {
            get { return (bool)GetValue(ShowExportFileProperty); }
            set { SetValue(ShowExportFileProperty, value); }
        }

        public static readonly DependencyProperty ShowExportFileProperty =
            DependencyProperty.Register("ShowExportFile", typeof(bool),
                typeof(InkControl), new PropertyMetadata(false));

        public bool ShowRecognize
        {
            get { return (bool)GetValue(ShowRecognizeProperty); }
            set { SetValue(ShowRecognizeProperty, value); }
        }

        public static readonly DependencyProperty ShowRecognizeProperty =
            DependencyProperty.Register("ShowRecognize",
                typeof(bool), typeof(InkControl), new PropertyMetadata(false));

    }
}
