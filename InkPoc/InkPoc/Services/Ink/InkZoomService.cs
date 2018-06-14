using System;
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

        public float ZoomIn(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor + zoomFactor);

        public float ZoomOut(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor - zoomFactor);

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

        private float ExecuteZoom(float zoomFactor)
        {
            if(scrollViewer.ChangeView(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset, zoomFactor))
            {
                return zoomFactor;
            }
            return scrollViewer.ZoomFactor;
        }
    }
}
