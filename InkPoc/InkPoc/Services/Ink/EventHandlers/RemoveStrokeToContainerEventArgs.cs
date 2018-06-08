using Windows.UI.Input.Inking;

namespace InkPoc.Services.Ink.EventHandlers
{
    public class RemoveStrokeToContainerEventArgs
    {
        public InkStroke RemovedStroke { get; set; }

        public RemoveStrokeToContainerEventArgs(InkStroke removedStroke)
        {
            RemovedStroke = removedStroke;
        }
    }
}
