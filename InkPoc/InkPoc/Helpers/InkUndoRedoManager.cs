using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers
{
    public class InkUndoRedoManager
    {
        private InkStrokeContainer _strokeContainer;

        public InkUndoRedoManager(InkStrokeContainer strokeContainer)
        {
            _strokeContainer = strokeContainer;            
        }

        private Stack<InkStroke> _strokesStack { get; set; } = new Stack<InkStroke>();

        public void ClearStack() => _strokesStack.Clear();

        public bool CanUndo => _strokeContainer != null && _strokeContainer.GetStrokes().Any();

        public bool CanRedo => _strokesStack.Any();

        public void Undo()
        {
            if(!CanUndo)
            {
                return;
            }

            var stroke = _strokeContainer.GetStrokes().Last();
            stroke.Selected = true;
            _strokesStack.Push(stroke);
            _strokeContainer.DeleteSelected();
        }   

        public void Redo()
        {
            if(!CanRedo)
            {
                return;
            }

            var stroke = _strokesStack.Pop();

            // _strokeContainer.AddStroke(stroke);

            var newStroke = CloneStroke(stroke);
            _strokeContainer.AddStroke(newStroke);
        }

        private InkStroke CloneStroke(InkStroke stroke)
        {
            var strokeBuilder = new InkStrokeBuilder();
            strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
            var newStroke = strokeBuilder.CreateStrokeFromInkPoints(stroke.GetInkPoints(), stroke.PointTransform);
            return newStroke;
        }
    }
}
