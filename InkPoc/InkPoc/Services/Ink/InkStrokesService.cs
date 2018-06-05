using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Media;

namespace InkPoc.Services.Ink
{
    public class InkStrokesService
    {
        public event EventHandler<AddStrokeToContainerEventArgs> AddStrokeEvent;

        private readonly InkStrokeContainer strokeContainer;

        public InkStrokesService(InkStrokeContainer _strokeContainer)
        {
            strokeContainer = _strokeContainer;
        }

        public InkStroke AddStrokeToContainer(InkStroke stroke)
        {
            var newStroke = stroke.Clone();
            strokeContainer.AddStroke(newStroke);

            AddStrokeEvent?.Invoke(this, new AddStrokeToContainerEventArgs(newStroke, stroke));

            return newStroke;
        }

        public void RemoveStrokeToContainer(InkStroke stroke)
        {
            var deleteStroke = strokeContainer
                .GetStrokes()
                .FirstOrDefault(s => s.Id == stroke.Id);

            if (deleteStroke != null)
            {
                ClearStrokesSelection();
                deleteStroke.Selected = true;
                strokeContainer.DeleteSelected();
            }
        }

        public IEnumerable<InkStroke> GetSelectedStrokes()
        {
            return strokeContainer.GetStrokes().Where(s => s.Selected);
        }

        public void ClearStrokes()
        {
            strokeContainer.Clear();
        }

        public void ClearStrokesSelection()
        {
            foreach (var stroke in strokeContainer.GetStrokes())
            {
                stroke.Selected = false;
            }
        }

        public Rect SelectStrokesByNode(IInkAnalysisNode node)
        {
            ClearStrokesSelection();
            var rect = node.BoundingRect;

            var strokeIds = GetNodeStrokeIds(node);
            foreach (var id in strokeIds)
            {
                var stroke = strokeContainer.GetStrokeById(id);
                stroke.Selected = true;
                rect.Union(stroke.BoundingRect);
            }

            return rect;
        }

        public Rect SelectStrokesByPoints(PointCollection points)
        {
            return strokeContainer.SelectWithPolyLine(points);
        }

        private IReadOnlyList<uint> GetNodeStrokeIds(IInkAnalysisNode node)
        {
            var strokeIds = node.GetStrokeIds();
            if (node.Kind == InkAnalysisNodeKind.Paragraph && node.Children[0].Kind == InkAnalysisNodeKind.ListItem)
            {
                strokeIds = new HashSet<uint>(strokeIds).ToList();
            }
            return strokeIds;
        }
    }

}
