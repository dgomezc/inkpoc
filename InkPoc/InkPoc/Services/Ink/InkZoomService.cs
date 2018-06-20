using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Services.Ink
{
    public class InkZoomService
    {
        private const float defaultZoomFactor = 0.1f;
        private readonly ScrollViewer scrollViewer;

        public InkZoomService(ScrollViewer _scrollViewer)
        {
            scrollViewer = _scrollViewer;
        }

        public float ZoomIn(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor + zoomFactor);

        public float ZoomOut(float zoomFactor = defaultZoomFactor) => ExecuteZoom(scrollViewer.ZoomFactor - zoomFactor);

        public void AdjustToSize(int width, int height)
        {
            if(width == 0 || height == 0)
            {
                return;
            }

            var ratioWidth = scrollViewer.ViewportWidth / width;
            var ratioHeight = scrollViewer.ViewportHeight / height;

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
