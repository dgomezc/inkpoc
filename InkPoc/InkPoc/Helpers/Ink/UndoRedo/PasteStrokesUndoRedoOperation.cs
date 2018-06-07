using InkPoc.Services.Ink;
using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class PasteStrokesUndoRedoOperation : AddStrokeUndoRedoOperation
    {
        public PasteStrokesUndoRedoOperation(IEnumerable<InkStroke> _strokes, InkStrokesService _strokeService)
            : base(_strokes, _strokeService)
        {
        }
    }
}
