using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink
{
    public class InkCopyPasteManager
    {
        private InkStrokeContainer _container;
            
        public InkCopyPasteManager(InkPresenter inkPresenter)
        {
            _container = inkPresenter.StrokeContainer;
        }

        public void Cut()
        {
            Copy();
            _container.DeleteSelected();
        }

        public void Copy() => _container.CopySelectedToClipboard();

        public void Paste(int xPosition = 0, int yPosition = 0)
        {
            if (_container.CanPasteFromClipboard())
            {
                var position = new Point(xPosition, yPosition);
                _container.PasteFromClipboard(position);
            }
        }
    }
}
