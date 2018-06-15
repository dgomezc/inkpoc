using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class SaveImageInkOption : InkOption
    {
        private const string SaveImageLabel = "Save image";

        private AppBarButton _saveImageButton;

        public AppBarButton SaveImageButton => _saveImageButton ?? (_saveImageButton = InkOptionHelper.BuildAppBarButton(SaveImageLabel, "EE71"));
    }
}
