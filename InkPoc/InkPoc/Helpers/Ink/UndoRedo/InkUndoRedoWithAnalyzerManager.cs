using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class InkUndoRedoWithAnalyzerManager
    {
        private readonly InkCanvas inkCanvas;
        private readonly InkAsyncAnalyzer analyzer;
        private readonly InkStrokeService strokeService;

        private Stack<IUndoRedoOperation> undoStack = new Stack<IUndoRedoOperation>();
        private Stack<IUndoRedoOperation> redoStack = new Stack<IUndoRedoOperation>();

        public InkUndoRedoWithAnalyzerManager(InkCanvas _inkCanvas, InkAsyncAnalyzer _analyzer)
        {
            inkCanvas = _inkCanvas;
            analyzer = _analyzer;

            strokeService = new InkStrokeService(inkCanvas.InkPresenter.StrokeContainer);

            inkCanvas.InkPresenter.StrokesCollected += (s, e) => AddOperation(new AddStrokeUndoRedoOperation(e.Strokes, strokeService));
            inkCanvas.InkPresenter.StrokesErased += (s, e) => AddOperation(new AddStrokeUndoRedoOperation(e.Strokes, strokeService), clearRedoStack: false);
        }

        public void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public bool CanUndo => undoStack.Any();

        public bool CanRedo => redoStack.Any();

        public void Undo()
        {
            if (!CanUndo)
                return;

            var element = undoStack.Pop();
            element.ExecuteUndo();
            redoStack.Push(element);
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var element = redoStack.Pop();
            element.ExecuteRedo();           
            undoStack.Push(element);
        }

        public void AddOperation(IUndoRedoOperation operation, bool clearRedoStack = true)
        {
            if (operation == null)
                return;

            undoStack.Push(operation);

            if (clearRedoStack)
            {
                redoStack.Clear();
            }

        }
    }

    public interface IUndoRedoOperation
    {
        void ExecuteUndo();

        void ExecuteRedo();
    }

    public class AddStrokeUndoRedoOperation : IUndoRedoOperation
    {
        private IReadOnlyList<InkStroke> strokes;
        private readonly InkStrokeService strokeService;

        public AddStrokeUndoRedoOperation(IReadOnlyList<InkStroke> _strokes, InkStrokeService _strokeService)
        {
            strokes = _strokes;
            strokeService = _strokeService;
        }

        public void ExecuteUndo()
        {
            foreach (var stroke in strokes)
            {
                strokeService.RemoveStrokeToContainer(stroke);
            }
        }

        public void ExecuteRedo()
        {
            var newStrokes = new List<InkStroke>();
            foreach (var stroke in strokes)
            {
                var newStroke = strokeService.AddStrokeToContainer(stroke);
                newStrokes.Add(newStroke);
            }

            strokes = newStrokes;
        }
    }

    public class RemoveStrokeUndoRedoOperation : IUndoRedoOperation
    {
        private IReadOnlyList<InkStroke> strokes;
        private readonly InkStrokeService strokeService;

        public RemoveStrokeUndoRedoOperation(IReadOnlyList<InkStroke> _strokes, InkStrokeService _strokeService)
        {
            strokes = _strokes;
            strokeService = _strokeService;
        }

        public void ExecuteRedo()
        {
            foreach (var stroke in strokes)
            {
                strokeService.RemoveStrokeToContainer(stroke);
            }
        }

        public void ExecuteUndo()
        {
            var newStrokes = new List<InkStroke>();
            foreach (var stroke in strokes)
            {
                var newStroke = strokeService.AddStrokeToContainer(stroke);
                newStrokes.Add(newStroke);
            }

            strokes = newStrokes;
        }
    }


    public class InkStrokeService
    {
        private readonly InkStrokeContainer strokeContainer;

        public InkStrokeService(InkStrokeContainer _strokeContainer)
        {
            strokeContainer = _strokeContainer;
        }        

        public InkStroke AddStrokeToContainer(InkStroke stroke)
        {
            var newStroke = stroke.Clone();
            strokeContainer.AddStroke(newStroke);

            ////// Update old strokes in stacks with new Stroke Id
            ////_undoStack
            ////   .Where(e => e.Stroke.Id == stroke.Id)
            ////   .ToList()
            ////   .ForEach(e => e.Stroke = newStroke);

            ////_redoStack
            ////    .Where(e => e.Stroke.Id == stroke.Id)
            ////    .ToList()
            ////    .ForEach(e => e.Stroke = newStroke);

            return newStroke;
        }

        public void RemoveStrokeToContainer(InkStroke stroke)
        {
            var deleteStroke = strokeContainer
                .GetStrokes()
                .FirstOrDefault(s => s.Id == stroke.Id);

            if (deleteStroke != null)
            {
                ClearStrokesSelection();
                deleteStroke.Selected = true;
                strokeContainer.DeleteSelected();
            }
        }

        public void ClearStrokesSelection()
        {
            foreach (var stroke in strokeContainer.GetStrokes())
            {
                stroke.Selected = false;
            }
        }
    }
}
