using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;

namespace InkPoc.Helpers.Ink
{
    public class InkTransformResult
    {
        public List<InkStroke> Strokes { get; set; } = new List<InkStroke>();

        public List<UIElement> TextAndShapes { get; set; } = new List<UIElement>();
    }
}
