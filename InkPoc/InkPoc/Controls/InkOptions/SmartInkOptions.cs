using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class SmartInkOptions : InkOption
    {
        private const string TransformTextAndShapesLabel = "Transform text and shapes";

        private AppBarButton _transformTextAndShapesButton;

        public AppBarButton TransformTextAndShapesButton => _transformTextAndShapesButton ?? (_transformTextAndShapesButton = BuildAppBarButton(TransformTextAndShapesLabel, "EA80"));
    }
}
