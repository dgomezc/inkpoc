using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class LassoSelectionInkOption : InkOption
    {
        public const string LassoSelectionButtonTag = "LassoSelection";
        private const string LassoSelectionLabel = "Selection tool";

        private InkToolbarCustomToolButton _lassoSelectionButton;

        public InkToolbarCustomToolButton LassoSelectionButton => _lassoSelectionButton ?? (_lassoSelectionButton = InkOptionHelper.BuildInkToolbarCustomToolButton(LassoSelectionLabel, "EF20", LassoSelectionButtonTag));
    }
}
