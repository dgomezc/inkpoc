using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class CutCopyPasteInkOption : InkOption
    {
        private const string CutLabel = "Cut";
        private const string CopyLabel = "Copy";
        private const string PasteLabel = "Paste";

        private AppBarButton _cutButton;
        private AppBarButton _copyButton;
        private AppBarButton _pasteButton;

        public AppBarButton CutButton => _cutButton ?? (_cutButton = InkOptionHelper.BuildAppBarButton(CutLabel, Symbol.Cut));

        public AppBarButton CopyButton => _copyButton ?? (_copyButton = InkOptionHelper.BuildAppBarButton(CopyLabel, Symbol.Copy));

        public AppBarButton PasteButton => _pasteButton ?? (_pasteButton = InkOptionHelper.BuildAppBarButton(PasteLabel, Symbol.Paste));
    }
}
