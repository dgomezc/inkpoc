using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class OpenImageInkOption : InkOption
    {
        private const string OpenImageLabel = "Open image";        

        private AppBarButton _openImageButton;

        public AppBarButton OpenImageButton => _openImageButton ?? (_openImageButton = BuildAppBarButton(OpenImageLabel, "EB9F"));
    }
}
