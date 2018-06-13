using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class UndoRedoInkOption : InkOption
    {
        private const string UndoLabel = "Undo";
        private const string RedoLabel = "Redo";

        private AppBarButton _undoButton;
        private AppBarButton _redoButton;

        public AppBarButton UndoButton => _undoButton ?? (_undoButton = BuildAppBarButton(UndoLabel, Symbol.Undo));

        public AppBarButton RedoButton => _redoButton ?? (_redoButton = BuildAppBarButton(RedoLabel, Symbol.Redo));
    }
}
