using System;
using System.Collections.Generic;
using System.Linq;
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
        ////private readonly DispatcherTimer dispatcherTimer;
        ////const double IDLE_WAITING_TIME = 400;
        ////const double BUSY_WAITING_TIME = 200;
        const double TRIPLE_TAP_TIME = 400;

        private readonly InkCanvas inkCanvas;
        private readonly InkPresenter inkPresenter;
        private readonly InkStrokeContainer strokeContainer;
        private InkAnalyzer inkAnalyzer;

        IInkAnalysisNode selectedNode;
        private readonly Canvas selectionCanvas;

        DateTime lastDoubleTapTime;

        public InkSelectionAndMoveManager(InkCanvas inkCanvas, Canvas selectionCanvas)
        {
            // Initialize properties
            this.inkCanvas = inkCanvas;
            this.selectionCanvas = selectionCanvas;
            inkPresenter = inkCanvas.InkPresenter;
            strokeContainer = inkPresenter.StrokeContainer;
            inkAnalyzer = new InkAnalyzer();

            // Register events
            this.inkCanvas.Tapped += InkCanvas_Tapped;
            this.inkCanvas.DoubleTapped += InkCanvas_DoubleTapped;


            ////// Perform analysis when there has been a change to the ink presenter and 
            ////// the user hasn't written a new stroke for IDLE_WAITING_TIME
            ////dispatcherTimer = new DispatcherTimer();
            ////dispatcherTimer.Tick += DispatcherTimer_Tick;
            ////dispatcherTimer.Interval = TimeSpan.FromMilliseconds(IDLE_WAITING_TIME);
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
