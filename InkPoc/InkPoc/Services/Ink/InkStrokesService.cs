using System;
using System.Linq;
using Windows.UI.Input.Inking;

namespace InkPoc.Services.Ink
{
    public class InkStrokesService
    {
        public event EventHandler<AddStrokeToContainerEventArgs> AddStrokeEvent;

        private readonly InkStrokeContainer strokeContainer;

        public InkStrokesService(InkStrokeContainer _strokeContainer)
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

}
