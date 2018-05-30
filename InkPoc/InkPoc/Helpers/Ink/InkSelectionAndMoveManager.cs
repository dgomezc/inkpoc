using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
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

        private bool enableLasso;
        private Polyline lasso;

        IInkAnalysisNode selectedNode;
        private readonly Canvas selectionCanvas;

        DateTime lastDoubleTapTime;
        Point dragStartPosition;

        public InkSelectionAndMoveManager(InkCanvas _inkCanvas, Canvas _selectionCanvas, InkAsyncAnalyzer _analyzer)
        {
            // Initialize properties
            inkCanvas = _inkCanvas;
            selectionCanvas = _selectionCanvas;
            inkPresenter = inkCanvas.InkPresenter;
            strokeContainer = inkPresenter.StrokeContainer;
            analyzer = _analyzer;

            // selection on tap
            this.inkCanvas.Tapped += InkCanvas_Tapped;
            this.inkCanvas.DoubleTapped += InkCanvas_DoubleTapped;

            //drag and drop
            inkCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            inkCanvas.PointerPressed += InkCanvas_PointerPressed;
            inkCanvas.ManipulationStarted += InkCanvas_ManipulationStarted;
            inkCanvas.ManipulationDelta += InkCanvas_ManipulationDelta;
            inkCanvas.ManipulationCompleted += InkCanvas_ManipulationCompleted;

            // lasso selection
            inkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkPresenter.StrokesErased += InkPresenter_StrokesErased;
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




        private void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            // Don't perform analysis while user is inking
            analyzer.StopTimer();

            // Quit lasso selection state
            EndLassoSelectionConfig();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            analyzer.StopTimer();
            foreach (var stroke in args.Strokes)
            {
                // Remove strokes from InkAnalyzer
                analyzer.InkAnalyzer.RemoveDataForStroke(stroke.Id);
            }
            analyzer.StartTimer();

            // Quit lasso selection state
            EndLassoSelectionConfig();
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            analyzer.StopTimer();
            analyzer.InkAnalyzer.AddDataForStrokes(args.Strokes);
            analyzer.StartTimer();
        }

        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso = new Polyline()
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
            };

            lasso.Points.Add(args.CurrentPoint.RawPosition);
            selectionCanvas.Children.Add(lasso);
            enableLasso = true;
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            if (enableLasso)
            {
                lasso.Points.Add(args.CurrentPoint.RawPosition);
            }
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);

            var rect = strokeContainer.SelectWithPolyLine(lasso.Points);
            enableLasso = false;

            selectionCanvas.Children.Remove(lasso);
            UpdateSelection(rect);
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

        public void ClearSelection()
        {
            selectionCanvas.Children.Clear();
            selectedNode = null;
        }

        public void StartLassoSelectionConfig()
        {
            inkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
        }

        public void EndLassoSelectionConfig()
        {
            inkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
        }
    }
}
