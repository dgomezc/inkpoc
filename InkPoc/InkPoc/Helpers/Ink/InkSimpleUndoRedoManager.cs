using InkPoc.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink
{
    public class InkSimpleUndoRedoManager
    {
        private InkStrokeContainer _strokeContainer;
        private Stack<InkStroke> _undoStack = new Stack<InkStroke>();

        public InkSimpleUndoRedoManager(InkPresenter inkPresenter)
        {
            _strokeContainer = inkPresenter.StrokeContainer;
        }

        public bool CanUndo => _strokeContainer.GetStrokes().Any();

        public bool CanRedo =>_undoStack.Any();

        public void Reset() => _undoStack.Clear();

        public void Undo()
        {
            if (!CanUndo)
                return;

            InkService.ClearStrokesSelection(_strokeContainer);
            var stroke = _strokeContainer.GetStrokes().Last();
            stroke.Selected = true;
            _strokeContainer.DeleteSelected();
            _undoStack.Push(stroke);
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var stroke = _undoStack.Pop();
            var newStroke = CopyStroke(stroke);
            _strokeContainer.AddStroke(newStroke);
        }

        private InkStroke CopyStroke(InkStroke stroke)
        {
            var strokeBuilder = new InkStrokeBuilder();
            strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
            var newStroke = strokeBuilder.CreateStrokeFromInkPoints(stroke.GetInkPoints(), stroke.PointTransform);

            return newStroke;
        }        
    }
}
