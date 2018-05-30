using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Helpers.Ink
{
    public class InkSelectionAndMoveManager
    {
        const double BUSY_WAITING_TIME = 200;
        const double TRIPLE_TAP_TIME = 400;

        private readonly InkCanvas inkCanvas;
        private readonly InkPresenter inkPresenter;
        private readonly InkStrokeContainer strokeContainer;
        private InkAsyncAnalyzer analyzer;

        IInkAnalysisNode selectedNode;
        private readonly Canvas selectionCanvas;

        DateTime lastDoubleTapTime;
        Point dragStartPosition;

        public InkSelectionAndMoveManager(InkCanvas inkCanvas, Canvas selectionCanvas)
        {
            // Initialize properties
            this.inkCanvas = inkCanvas;
            this.selectionCanvas = selectionCanvas;
            inkPresenter = inkCanvas.InkPresenter;
            strokeContainer = inkPresenter.StrokeContainer;
            analyzer = new InkAsyncAnalyzer(strokeContainer);

            // selection on tap
            this.inkCanvas.Tapped += InkCanvas_Tapped;
            this.inkCanvas.DoubleTapped += InkCanvas_DoubleTapped;

            //drag and drop
            inkCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            inkCanvas.PointerPressed += InkCanvas_PointerPressed;
            inkCanvas.ManipulationStarted += InkCanvas_ManipulationStarted;
            inkCanvas.ManipulationDelta += InkCanvas_ManipulationDelta;
            inkCanvas.ManipulationCompleted += InkCanvas_ManipulationCompleted;
        }

        private void InkCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var position = e.GetPosition(inkCanvas);

            if (selectedNode != null && RectHelper.Contains(selectedNode.BoundingRect, position))
            {
                if (DateTime.Now.Subtract(lastDoubleTapTime).TotalMilliseconds < TRIPLE_TAP_TIME)
                {
                    ExpandSelection();
                }
            }
            else
            {
                var inkAnalyzer = analyzer.InkAnalyzer; //??
                selectedNode = InkHelper.FindHitNode(ref inkAnalyzer, position, strokeContainer);
                ShowOrHideSelection(selectedNode);
            }
        }

        private void InkCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(inkCanvas);

            if (selectedNode != null && RectHelper.Contains(selectedNode.BoundingRect, position))
            {
                ExpandSelection();
                lastDoubleTapTime = DateTime.Now;
            }
        }





        private async void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(inkCanvas).Position;
            while (analyzer.IsAnalyzing)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(BUSY_WAITING_TIME));
            }

            if ((selectedNode != null) && (RectHelper.Contains(selectedNode.BoundingRect, position)))
            {
                // Pressed on the selected node, do nothing
                return;
            }

            var inkAnalyzer = analyzer.InkAnalyzer; //??
            selectedNode = InkHelper.FindHitNode(ref inkAnalyzer, position, strokeContainer);
            ShowOrHideSelection(selectedNode);
        }

        private void InkCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (selectedNode != null)
            {
                dragStartPosition = e.Position;
            }
        }

        private void InkCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (selectedNode != null)
            {
                Point offset;
                offset.X = e.Delta.Translation.X;
                offset.Y = e.Delta.Translation.Y;
                MoveSelection(offset);
            }
        }

        private async void InkCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (selectedNode != null)
            {
                MoveInk(e.Position);

                ////var mx = InkHelper.GetTranslationMX(e.Position.X - dragStartPosition.X, e.Position.Y - dragStartPosition.Y);
                ////InkHelper.TransformInk(strokeContainer, selectedNode, mx);
                ////InkHelper.UpdateInkForNode(inkAnalyzer, strokeContainer, selectedNode);

                // Strokes are moved and the analysis result is not valid anymore.
                await analyzer.AnalyzeAsync();
            }
        }




        private void MoveInk(Point position)
        {
            var x = (float)(position.X - dragStartPosition.X);
            var y = (float)(position.Y - dragStartPosition.Y);

            var matrix = Matrix3x2.CreateTranslation(x, y);

            if (!matrix.IsIdentity)
            {
                var strokeIds = InkHelper.GetNodeStrokeIds(selectedNode);
                foreach (var id in strokeIds)
                {
                    var stroke = strokeContainer.GetStrokeById(id);
                    stroke.PointTransform *= matrix;
                    analyzer.InkAnalyzer.ReplaceDataForStroke(strokeContainer.GetStrokeById(id));
                }
            }
        }

        private void ExpandSelection()
        {
            if (selectedNode != null &&
                selectedNode.Kind != InkAnalysisNodeKind.UnclassifiedInk &&
                selectedNode.Kind != InkAnalysisNodeKind.InkDrawing &&
                selectedNode.Kind != InkAnalysisNodeKind.WritingRegion)
            {
                selectedNode = selectedNode.Parent;
                if (selectedNode.Kind == InkAnalysisNodeKind.ListItem && selectedNode.Children.Count == 1)
                {
                    // Hierarchy: WritingRegion->Paragraph->ListItem->Line->{Bullet, Word1, Word2...}
                    // When a ListItem has only one Line, the bounding rect is same with its child Line,
                    // in this case, we skip one level to avoid confusion.
                    selectedNode = selectedNode.Parent;
                }

                ShowOrHideSelection(selectedNode);
            }
        }

        private void ShowOrHideSelection(IInkAnalysisNode node)
        {
            if (node == null)
            {
                ClearSelection();
                return;
            }

            UpdateSelection(node.BoundingRect);
        }        

        private void UpdateSelection(Rect rect)
        {
            var selectionRect = GetSelectionRectangle();

            selectionRect.Width = rect.Width;
            selectionRect.Height = rect.Height;
            Canvas.SetLeft(selectionRect, rect.Left);
            Canvas.SetTop(selectionRect, rect.Top);
        }

        private void MoveSelection(Point offset)
        {
            var selectionRect = GetSelectionRectangle();

            var left = Canvas.GetLeft(selectionRect);
            var top = Canvas.GetTop(selectionRect);
            Canvas.SetLeft(selectionRect, left + offset.X);
            Canvas.SetTop(selectionRect, top + offset.Y);
        }

        private Rectangle GetSelectionRectangle()
        {
            var selectionRectange = selectionCanvas.Children.FirstOrDefault(f => f is Rectangle r && r.Name == "selectionRectangle") as Rectangle;

            if(selectionRectange == null)
            {
                selectionRectange = new Rectangle()
                {
                    Name = "selectionRectangle",
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection() { 2, 2 },
                    StrokeDashCap = PenLineCap.Round
                };

                selectionCanvas.Children.Add(selectionRectange);
            }
            
            return selectionRectange;
        }

        private void ClearSelection() => selectionCanvas.Children.Clear();
    }
}
