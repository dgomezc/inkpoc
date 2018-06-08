using System;
using Windows.UI.Input.Inking;

namespace InkPoc.Services.Ink.EventHandlers
{
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
