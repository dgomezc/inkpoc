using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InkPoc.Controls
{
    public abstract class InkOption : FrameworkElement
    {
        protected AppBarButton BuildAppBarButton(string label, string codeString)
        {
            int code = int.Parse(codeString, NumberStyles.HexNumber);
            var icon = new FontIcon()
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = char.ConvertFromUtf32(code)
            };
            return BuildAppBarButton(label, icon);
        }

        protected AppBarButton BuildAppBarButton(string label, Symbol icon)
        {
            return BuildAppBarButton(label, new SymbolIcon(icon));
        }

        private AppBarButton BuildAppBarButton(string label, IconElement icon)
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
    }
}
