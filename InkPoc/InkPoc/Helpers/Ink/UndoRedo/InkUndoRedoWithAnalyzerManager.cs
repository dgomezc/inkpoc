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

    public interface IUndoRedoOperation
    {
        void ExecuteUndo();

        void ExecuteRedo();
    }

    public class AddStrokeUndoRedoOperation : IUndoRedoOperation
    {
        private List<InkStroke> strokes;
        private readonly InkStrokeService strokeService;

        public AddStrokeUndoRedoOperation(IEnumerable<InkStroke> _strokes, InkStrokeService _strokeService)
        {
            strokes = new List<InkStroke>(_strokes);
            strokeService = _strokeService;

            strokeService.AddStrokeEvent += StrokeService_AddStrokeEvent;
        }
        
        public void ExecuteUndo() => strokes.ForEach(s => strokeService.RemoveStrokeToContainer(s));

        public void ExecuteRedo() => strokes.ToList().ForEach(s => strokeService.AddStrokeToContainer(s));

        private void StrokeService_AddStrokeEvent(object sender, AddStrokeToContainerEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var removedStrokes = strokes.RemoveAll(s => s.Id == e.OldStroke?.Id);
            if (removedStrokes > 0)
            {
                strokes.Add(e.NewStroke);
            }
        }
    }

    public class RemoveStrokeUndoRedoOperation : IUndoRedoOperation
    {
        private List<InkStroke> strokes;
        private readonly InkStrokeService strokeService;

        public RemoveStrokeUndoRedoOperation(IEnumerable<InkStroke> _strokes, InkStrokeService _strokeService)
        {
            strokes = new List<InkStroke>(_strokes);
            strokeService = _strokeService;

            strokeService.AddStrokeEvent += StrokeService_AddStrokeEvent;
        }

        public void ExecuteRedo() => strokes.ForEach(s => strokeService.RemoveStrokeToContainer(s));

        public void ExecuteUndo() => strokes.ToList().ForEach(s => strokeService.AddStrokeToContainer(s));

        private void StrokeService_AddStrokeEvent(object sender, AddStrokeToContainerEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var changes = strokes.RemoveAll(s => s.Id == e.OldStroke?.Id);
            if (changes > 0)
            {
                strokes.Add(e.NewStroke);
            }
        }
    }
    
    public class InkStrokeService
    {
        public event EventHandler<AddStrokeToContainerEventArgs> AddStrokeEvent;

        private readonly InkStrokeContainer strokeContainer;

        public InkStrokeService(InkStrokeContainer _strokeContainer)
        {
            strokeContainer = _strokeContainer;
        }        

        public InkStroke AddStrokeToContainer(InkStroke stroke)
        {
            var newStroke = stroke.Clone();
            strokeContainer.AddStroke(newStroke);

            AddStrokeEvent?.Invoke(this, new AddStrokeToContainerEventArgs(newStroke, stroke));

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

    public class AddStrokeToContainerEventArgs : EventArgs
    {
        public InkStroke OldStroke { get; set; }
        public InkStroke NewStroke { get; set; }

        public AddStrokeToContainerEventArgs(InkStroke newStroke, InkStroke oldStroke = null)
        {
            NewStroke = newStroke;
            OldStroke = oldStroke;
        }
    }
}
