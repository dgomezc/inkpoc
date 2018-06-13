using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class ZoomInkOption : InkOption
    {
        private const string ZoomInLabel = "Zoom in";
        private const string ZoomOutLabel = "Zoom out";

        private AppBarButton _zoomInButton;
        private AppBarButton _zoomOutButton;

        public AppBarButton ZoomInButton => _zoomInButton ?? (_zoomInButton = BuildAppBarButton(ZoomInLabel, Symbol.ZoomIn));

        public AppBarButton ZoomOutButton => _zoomOutButton ?? (_zoomOutButton = BuildAppBarButton(ZoomOutLabel, Symbol.ZoomOut));
    }
}
