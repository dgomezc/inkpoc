using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Media;

namespace InkPoc.Services.Ink
{
    public class InkStrokesService
    {
        public event EventHandler<AddStrokeToContainerEventArgs> AddStrokeEvent;
        public event EventHandler<RemoveStrokeToContainerEventArgs> RemoveStrokeEvent;
        public event EventHandler<MoveStrokesEventArgs> MoveStrokesEvent;
        public event EventHandler<EventArgs> ClearStrokesEvent;

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

        public bool RemoveStrokeToContainer(InkStroke stroke)
        {
            var deleteStroke = GetStrokes().FirstOrDefault(s => s.Id == stroke.Id);
            if (deleteStroke == null)
            {
                return false;
            }

            ClearStrokesSelection();
            deleteStroke.Selected = true;
            strokeContainer.DeleteSelected();

            RemoveStrokeEvent?.Invoke(this, new RemoveStrokeToContainerEventArgs(stroke));
            return true;
        }

        public bool RemoveStrokesByStrokeIds(IEnumerable<uint> strokeIds)
        {
            var strokes = GetStrokesByIds(strokeIds);

            foreach(var stroke in strokes)
            {
                RemoveStrokeToContainer(stroke);
            }

            return strokes.Any();
        }

        public IEnumerable<InkStroke> GetStrokes() => strokeContainer.GetStrokes();

        public IEnumerable<InkStroke> GetSelectedStrokes() => GetStrokes().Where(s => s.Selected);

        private IEnumerable<InkStroke> GetStrokesByIds(IEnumerable<uint> strokeIds)
        {
            foreach (var strokeId in strokeIds)
            {
                yield return strokeContainer.GetStrokeById(strokeId);
            }
        }

        public void ClearStrokes()
        {
            strokeContainer.Clear();
            ClearStrokesEvent?.Invoke(this, null);
        }

        public void ClearStrokesSelection()
        {
            foreach (var stroke in GetStrokes())
            {
                stroke.Selected = false;
            }
        }

        public Rect SelectStrokes(IEnumerable<InkStroke> strokes)
        {
            ClearStrokesSelection();
            var rect = Rect.Empty;

            var strokeIds = strokes.Select(s => s.Id);
            foreach (var id in strokeIds)
            {
                var stroke = strokeContainer.GetStrokeById(id);
                stroke.Selected = true;
                rect.Union(stroke.BoundingRect);
            }

            return rect;
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

        public void MoveSelectedStrokes(Point startPosition, Point endPosition)
        {
            var x = (float)(endPosition.X - startPosition.X);
            var y = (float)(endPosition.Y - startPosition.Y);

            var matrix = Matrix3x2.CreateTranslation(x, y);

            if (!matrix.IsIdentity)
            {
                var selectedStrokes = GetSelectedStrokes();
                foreach (var stroke in selectedStrokes)
                {
                    stroke.PointTransform *= matrix;
                }

                MoveStrokesEvent?.Invoke(this, new MoveStrokesEventArgs(selectedStrokes, startPosition, endPosition));
            }
        }
    }
}
