using InkPoc.Services.Ink;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class InkUndoRedoManager
    {
        private readonly InkCanvas inkCanvas;
        private readonly InkAsyncAnalyzer analyzer;
        private readonly InkStrokesService strokeService;

        private Stack<IUndoRedoOperation> undoStack = new Stack<IUndoRedoOperation>();
        private Stack<IUndoRedoOperation> redoStack = new Stack<IUndoRedoOperation>();

        public InkUndoRedoManager(InkCanvas _inkCanvas, InkAsyncAnalyzer _analyzer)
        {
            inkCanvas = _inkCanvas;
            analyzer = _analyzer;

            strokeService = new InkStrokesService(inkCanvas.InkPresenter.StrokeContainer);

            inkCanvas.InkPresenter.StrokesCollected += (s, e) => AddOperation(new AddStrokeUndoRedoOperation(e.Strokes, strokeService));
            inkCanvas.InkPresenter.StrokesErased += (s, e) => AddOperation(new RemoveStrokeUndoRedoOperation(e.Strokes, strokeService), clearRedoStack: false);
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
}
