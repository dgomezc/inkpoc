using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class ClearAllInkOption : InkOption
    {
        private const string ClearAllLabel = "Clear all";

        private AppBarButton _clearAllButton;

        public AppBarButton ClearAllButton => _clearAllButton ?? (_clearAllButton = BuildAppBarButton(ClearAllLabel, Symbol.Delete));
    }
}
