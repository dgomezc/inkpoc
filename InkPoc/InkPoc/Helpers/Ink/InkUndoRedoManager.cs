using InkPoc.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink
{
    public class InkUndoRedoManager
    {
        private InkStrokeContainer _strokeContainer;
        private Stack<UndoRedoElement> _undoStack = new Stack<UndoRedoElement>();
        private Stack<UndoRedoElement> _redoStack  = new Stack<UndoRedoElement>();

        public InkUndoRedoManager(InkPresenter inkPresenter)
        {
            _strokeContainer = inkPresenter.StrokeContainer;

            inkPresenter.StrokesCollected += (s, e) => AddToUndoStack(e.Strokes, UndoRedoOperation.Add, clearRedoStack: true);
            inkPresenter.StrokesErased += (s, e) => AddToUndoStack(e.Strokes, UndoRedoOperation.Remove);
        }

        public void Reset()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
        
        public bool CanUndo => _undoStack.Any();

        public bool CanRedo => _redoStack.Any();

        public void Undo()
        {
            if (!CanUndo)
                return;
            
            var element = _undoStack.Pop();

            if (element.Operation == UndoRedoOperation.Add)
            {
                RemoveStrokeToContainer(element.Stroke);
            }
            else
            {
                var newStroke = AddStrokeToContainer(element.Stroke);
                element.Stroke = newStroke;
            }

            _redoStack.Push(element);
        }

        public void Redo()
        {
            if(!CanRedo)
                return;

            var element = _redoStack.Pop();

            if (element.Operation == UndoRedoOperation.Add)
            {
                var newStroke = AddStrokeToContainer(element.Stroke);
                element.Stroke = newStroke;
            }
            else
            {
                RemoveStrokeToContainer(element.Stroke);
            }

            _undoStack.Push(element);
        }

        private InkStroke CopyStroke(InkStroke stroke)
        {
            var strokeBuilder = new InkStrokeBuilder();
            strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
            var newStroke = strokeBuilder.CreateStrokeFromInkPoints(stroke.GetInkPoints(), stroke.PointTransform);

            return newStroke;
        }

        private InkStroke AddStrokeToContainer(InkStroke stroke)
        {
            var newStroke = CopyStroke(stroke);
            _strokeContainer.AddStroke(newStroke);

            // Update old strokes in stacks with new Stroke Id
            _undoStack
               .Where(e => e.Stroke.Id == stroke.Id)
               .ToList()
               .ForEach(e => e.Stroke = newStroke);

            _redoStack
                .Where(e => e.Stroke.Id == stroke.Id)
                .ToList()
                .ForEach(e => e.Stroke = newStroke);

            return newStroke;
        }

        private void RemoveStrokeToContainer(InkStroke stroke)
        {
            var deleteStroke = _strokeContainer
                .GetStrokes()
                .FirstOrDefault(s => s.Id == stroke.Id);

            if (deleteStroke != null)
            {
                InkService.ClearStrokesSelection(_strokeContainer);
                deleteStroke.Selected = true;
                _strokeContainer.DeleteSelected();
            }
        }

        private void AddToUndoStack(IEnumerable<InkStroke> strokes, UndoRedoOperation operation, bool clearRedoStack = false)
        {
            foreach (var stroke in strokes)
            {
                var element = new UndoRedoElement(stroke, operation);
                _undoStack.Push(element);
            }

            if (clearRedoStack)
            {
                _redoStack.Clear();
            }
        }
    }
}
