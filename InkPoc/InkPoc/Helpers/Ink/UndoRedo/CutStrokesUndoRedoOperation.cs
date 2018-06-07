using InkPoc.Services.Ink;
using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class CutStrokesUndoRedoOperation : RemoveStrokeUndoRedoOperation
    {
        public CutStrokesUndoRedoOperation(IEnumerable<InkStroke> _strokes, InkStrokesService _strokeService)
               : base(_strokes, _strokeService)
        {
        }
    }
}
