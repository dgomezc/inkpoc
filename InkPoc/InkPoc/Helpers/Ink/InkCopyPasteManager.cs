using InkPoc.Services.Ink;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink
{
    public class InkCopyPasteManager
    {
        private Point pastePosition;
        private const int PASTE_DISTANCE = 20;
        private readonly InkStrokesService strokesService;

        public InkCopyPasteManager(InkStrokesService _strokesService)
        {
            strokesService = _strokesService;
        }

        public Point Copy()
        {
            var rect = strokesService.CopySelectedStrokes();

            pastePosition = new Point(rect.X, rect.Y);
            return pastePosition;
        }

        public Point Cut()
        {
            var rect = strokesService.CutSelectedStrokes();

            pastePosition = new Point(rect.X, rect.Y);
            return pastePosition;
        }


        public Rect Paste()
        {
            pastePosition.X += PASTE_DISTANCE;
            pastePosition.Y += PASTE_DISTANCE;

            return Paste(pastePosition);
        }

        public Rect Paste(Point position) => strokesService.PasteSelectedStrokes(position);

        public bool CanCopy => strokesService.GetSelectedStrokes().Any();

        public bool CanCut => strokesService.GetSelectedStrokes().Any();

        public bool CanPaste => strokesService.CanPaste;

    }
}
