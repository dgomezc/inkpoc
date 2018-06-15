using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class OpenSaveStrokesInkOption : InkOption
    {
        private const string OpenStrokesFileLabel = "Open strokes";
        private const string SaveStrokesFileLabel = "Save strokes";

        private AppBarButton _openStrokesButton;
        private AppBarButton _saveStrokesButton;

        public AppBarButton OpenStrokesButton => _openStrokesButton ?? (_openStrokesButton = InkOptionHelper.BuildAppBarButton(OpenStrokesFileLabel, "E7C3"));

        public AppBarButton SaveStrokesButton => _saveStrokesButton ?? (_saveStrokesButton = InkOptionHelper.BuildAppBarButton(SaveStrokesFileLabel, "E792"));
    }
}
