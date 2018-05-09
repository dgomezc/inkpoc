using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink
{
    internal class UndoRedoElement
    {
        public UndoRedoElement(InkStroke stroke, UndoRedoOperation operation)
        {
            Stroke = stroke;
            Operation = operation;
        }

        public UndoRedoOperation Operation { get; set; }

        public InkStroke Stroke { get; set; }
    }
}
