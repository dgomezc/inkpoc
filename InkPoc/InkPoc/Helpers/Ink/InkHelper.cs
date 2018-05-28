using System.Numerics;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using System.Linq;


namespace InkPoc.Helpers.Ink
{
    public enum AlignmentOption
    {
        Left,
        Center,
        Right,
    }

    public class InkHelper
    {
        #region Touch Helpers
        public static bool IfAContainsB(Rect A, Rect B)
        {
            // TODOYIBOSUN: implement in real
            bool contained = false;
            var intersect = RectHelper.Intersect(A, B);
            if (!intersect.IsEmpty)
            {
                var areaIntersect = intersect.Width * intersect.Height;
                var areaB = B.Width * B.Height;
                contained = ((areaIntersect / areaB) > 0.8);
            }
            return contained;
        }

        private static IInkAnalysisNode FindHitNodeByKind(ref InkAnalyzer ia, Point pt, InkAnalysisNodeKind kind, InkStrokeContainer strokeContainer = null)
        {
            var nodes = ia.AnalysisRoot.FindNodes(kind);
            foreach (var node in nodes)
            {
                var rect = (strokeContainer == null) ? node.BoundingRect : GetCurrentBoundingRectOfNode(strokeContainer, node);
                if (RectHelper.Contains(rect, pt))
                {
                    return node;
                }
            }
            return null;
        }

        public static IInkAnalysisNode FindHitNode(ref InkAnalyzer ia, Point pt, InkStrokeContainer strokeContainer = null)
        {
            // Start with smallest scope
            var node = FindHitNodeByKind(ref ia, pt, InkAnalysisNodeKind.InkWord, strokeContainer);
            if (node == null)
            {
                node = FindHitNodeByKind(ref ia, pt, InkAnalysisNodeKind.InkBullet, strokeContainer);
                if (node == null)
                {
                    node = FindHitNodeByKind(ref ia, pt, InkAnalysisNodeKind.InkDrawing, strokeContainer);
                }
            }
            return node;
        }
        #endregion

        #region Alignment Helper
        public static Rect GetCurrentBoundingRectOfNode(InkStrokeContainer strokeContainer, IInkAnalysisNode node)
        {
            Rect rect = new Rect();
            rect.Width = 0;
            var strokeIds = GetNodeStrokeIds(node);
            foreach (var id in strokeIds)
            {
                var stroke = strokeContainer.GetStrokeById(id);
                if (rect.Width == 0)
                {
                    rect = stroke.BoundingRect;
                }
                else
                {
                    rect = RectHelper.Union(rect, stroke.BoundingRect);
                }
            }
            return rect;
        }

        public static Matrix3x2 ComputeAlignmentMX(Rect rect, double alignmentReference, AlignmentOption option, double tolerance = 0)
        {
            Matrix3x2 mx = Matrix3x2.Identity;
            double shift = 0;

            switch (option)
            {
                case AlignmentOption.Left:
                    shift = alignmentReference - rect.Left;
                    break;
                case AlignmentOption.Right:
                    shift = alignmentReference - rect.Right;
                    break;
                case AlignmentOption.Center:
                    shift = alignmentReference - (rect.Left + rect.Right) / 2;
                    break;
                default:
                    break;
            }

            if (System.Math.Abs(shift) > tolerance)
            {
                mx = GetTranslationMX(shift, 0);
            }
            return mx;
        }

        public static Matrix3x2 GetTranslationMX(double x, double y)
        {
            return Matrix3x2.CreateTranslation((float)x, (float)y);
        }

        public static Matrix3x2 GetScaleMX(double x, double y, double scaleFactor)
        {
            var mx1 = GetTranslationMX(-x, -y);
            var mx2 = Matrix3x2.CreateScale((float)scaleFactor);
            var mx3 = GetTranslationMX(x, y);
            return mx1 * mx2 * mx3;
        }

        public static double GetRotationAngle(IInkAnalysisNode node)
        {
            var rrect = node.RotatedBoundingRect;
            var TopLeft = rrect[0];
            var TopRight = rrect[1];

            var dy = TopRight.Y - TopLeft.Y;
            var dx = TopRight.X - TopLeft.X;
            var theta = -1 * System.Math.Atan2(dy, dx);
            return theta;
        }

        public static Matrix3x2 GetRotationMX(IInkAnalysisNode node, double theta = 0)
        {
            if (theta == 0)
            {
                theta = GetRotationAngle(node);
            }
            var rect = node.BoundingRect;
            var center = new Vector2((float)(rect.Left + rect.Right) / 2, (float)(rect.Top + rect.Bottom) / 2);
            return Matrix3x2.CreateRotation((float)theta, center);
        }

        private static void TransformInkById(InkStrokeContainer strokeContainer, IReadOnlyList<uint> strokeIds, Matrix3x2 mx)
        {
            foreach (var id in strokeIds)
            {
                var stroke = strokeContainer.GetStrokeById(id);
                stroke.PointTransform *= mx;
            }
        }

        public static void TransformInk(InkStrokeContainer strokeContainer, IInkAnalysisNode node, System.Numerics.Matrix3x2 mx)
        {
            if (!mx.IsIdentity)
            {
                var strokeIds = GetNodeStrokeIds(node);
                TransformInkById(strokeContainer, strokeIds, mx);
            }
        }

        public static void AlignBaseline(InkStrokeContainer strokes, IInkAnalysisNode node)
        {
            if (node != null && node.Kind == InkAnalysisNodeKind.Line)
            {
                double baseline = node.BoundingRect.Bottom;
                foreach (var word in node.Children)
                {
                    double offsetY = word.BoundingRect.Bottom - baseline;

                    var mx = Matrix3x2.CreateTranslation(0, (float)offsetY);
                    InkHelper.TransformInk(strokes, word, mx);
                }
            }
        }
        #endregion

        #region Styling Helper
        public static void ChangeInkColorByNode(InkStrokeContainer strokeContainer, IInkAnalysisNode node, Color color)
        {
            var strokeIds = GetNodeStrokeIds(node);
            foreach (var id in strokeIds)
            {
                var stroke = strokeContainer.GetStrokeById(id);
                var drawingAttributes = stroke.DrawingAttributes;
                drawingAttributes.Color = color;
                stroke.DrawingAttributes = drawingAttributes;
            }
        }
        #endregion

        #region InkAnalyzer Operation Simplifier
        public static IReadOnlyList<uint> GetNodeStrokeIds(IInkAnalysisNode node)
        {
            var strokeIds = node.GetStrokeIds();
            if (node.Kind == InkAnalysisNodeKind.Paragraph && node.Children[0].Kind == InkAnalysisNodeKind.ListItem)
            {
                strokeIds = new HashSet<uint>(strokeIds).ToList<uint>();
            }
            return strokeIds;
        }

        public static void RemoveInkForNode(InkAnalyzer inkAnalyzer, InkStrokeContainer strokeContainer, IInkAnalysisNode node)
        {
            var strokeIds = GetNodeStrokeIds(node);
            inkAnalyzer.RemoveDataForStrokes(strokeIds);

            foreach (var id in strokeIds)
            {
                strokeContainer.GetStrokeById(id).Selected = true;
            }
            strokeContainer.DeleteSelected();
        }

        public static void UpdateInkForNode(InkAnalyzer inkAnalyzer, InkStrokeContainer strokeContainer, IInkAnalysisNode node)
        {
            var strokeIds = GetNodeStrokeIds(node);
            foreach (var id in strokeIds)
            {
                inkAnalyzer.ReplaceDataForStroke(strokeContainer.GetStrokeById(id));
            }
        }

        public static void ExtractWordsAndBullets(IInkAnalysisNode node, ref List<IInkAnalysisNode> words)
        {
            if (node.Kind == InkAnalysisNodeKind.InkDrawing ||
                node.Kind == InkAnalysisNodeKind.UnclassifiedInk)
            {
                return;
            }

            if (node.Kind == InkAnalysisNodeKind.InkWord ||
                node.Kind == InkAnalysisNodeKind.InkBullet)
            {
                words.Add(node);
                return;
            }

            foreach (var child in node.Children)
            {
                ExtractWordsAndBullets(child, ref words);
            }
        }

        public static List<IInkAnalysisNode> ExtractWordNodesInParagraph(IInkAnalysisNode node)
        {
            List<IInkAnalysisNode> words = new List<IInkAnalysisNode>();
            if (node.Kind == InkAnalysisNodeKind.Paragraph)
            {
                ExtractWordsAndBullets(node, ref words);
            }
            return words;
        }
        #endregion
    }
}
