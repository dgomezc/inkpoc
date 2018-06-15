using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InkPoc.Controls
{
    internal static class InkOptionHelper
    {
        internal static AppBarButton BuildAppBarButton(string label, string codeString)
        {
            int code = int.Parse(codeString, NumberStyles.HexNumber);
            var icon = new FontIcon()
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = char.ConvertFromUtf32(code)
            };
            return BuildAppBarButton(label, icon);
        }

        internal static AppBarButton BuildAppBarButton(string label, Symbol icon)
        {
            return BuildAppBarButton(label, new SymbolIcon(icon));
        }

        internal static InkToolbarCustomToolButton BuildInkToolbarCustomToolButton(string label, string codeString, string tag = null)
        {
            int code = int.Parse(codeString, NumberStyles.HexNumber);
            var icon = new FontIcon()
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = char.ConvertFromUtf32(code)
            };
            return BuildInkToolbarCustomToolButton(label, icon, tag);
        }

        internal static InkToolbarCustomToolButton BuildInkToolbarCustomToolButton(string label, Symbol icon, string tag = null)
        {
            return BuildInkToolbarCustomToolButton(label, new SymbolIcon(icon), tag);
        }

        internal static AppBarButton BuildAppBarButton(string label, IconElement icon)
        {
            var appBarButton = new AppBarButton()
            {
                Label = label,
                Icon = icon,
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            ToolTipService.SetToolTip(appBarButton, label);
            return appBarButton;
        }

        internal static InkToolbarCustomToolButton BuildInkToolbarCustomToolButton(string label, IconElement icon, string tag = null)
        {
            var inkToolbarCustomToolButton = new InkToolbarCustomToolButton()
            {
                Content = icon,
                Tag = tag
            };
            ToolTipService.SetToolTip(inkToolbarCustomToolButton, label);
            return inkToolbarCustomToolButton;
        }
    }
}
