using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace InkPoc.Services.Ink
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
