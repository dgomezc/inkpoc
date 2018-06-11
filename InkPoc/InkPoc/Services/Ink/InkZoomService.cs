using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Services.Ink
{
    public class InkZoomService
    {
        private const float defaultZoomFactor = 0.2f;
        private readonly ScrollViewer scrollViewer;

        public InkZoomService(ScrollViewer _scrollViewer)
        {
            scrollViewer = _scrollViewer;
        }

        public void ZoomIn(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor + zoomFactor);

        public void ZoomOut(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor - zoomFactor);

        public void AdjustToSize(Size size)
        {
            if(size.IsEmpty)
            {
                return;
            }

            var ratioWidth = scrollViewer.ViewportWidth / size.Width;
            var ratioHeight = scrollViewer.ViewportHeight / size.Height;

            var zoomFactor = (ratioWidth >= 1 && ratioHeight >= 1)
                ? 1F
                : (float)(Math.Min(ratioWidth, ratioHeight));

            ExecuteZoom(zoomFactor);
        }

        private void ExecuteZoom(float zoomFactor) => scrollViewer.ChangeView(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset, zoomFactor);
    }
}
