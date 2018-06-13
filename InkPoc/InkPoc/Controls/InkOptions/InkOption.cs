using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public abstract class InkOption : FrameworkElement
    {

        protected AppBarButton BuildAppBarButton(string label, Symbol icon)
        {
            var appBarButton = new AppBarButton()
            {
                Label = label,
                Icon = new SymbolIcon(icon),
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            ToolTipService.SetToolTip(appBarButton, label);
            return appBarButton;
        }
    }
}
