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
                
        public static Matrix3x2 GetTranslationMX(double x, double y)
        {
            return Matrix3x2.CreateTranslation((float)x, (float)y);
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
        
        #endregion
    }
}
