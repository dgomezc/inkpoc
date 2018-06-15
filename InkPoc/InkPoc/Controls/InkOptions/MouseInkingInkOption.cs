using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class MouseInkingInkOption :  InkOption
    {
        public const string MouseInkingButtonTag = "MouseInking";
        private const string MouseInkingLabel = "Mouse inking";

        private InkToolbarCustomToggleButton _mouseInkingButton;

        public InkToolbarCustomToggleButton MouseInkingButton => _mouseInkingButton ?? (_mouseInkingButton = InkOptionHelper.BuildInkToolbarCustomToggleButton(MouseInkingLabel, "E962", MouseInkingButtonTag));
    }
}
