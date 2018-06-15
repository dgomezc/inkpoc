using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class TouchInkingInkOption : InkOption
    {
        public const string TouchInkingButtonTag = "TouchInking";
        private const string TouchInkingLabel = "Touch inking";

        private InkToolbarCustomToggleButton _touchInkingButton;

        public InkToolbarCustomToggleButton TouchInkingButton => _touchInkingButton ?? (_touchInkingButton = InkOptionHelper.BuildInkToolbarCustomToggleButton(TouchInkingLabel, "ED5F", TouchInkingButtonTag));
    }
}
