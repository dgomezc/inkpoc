using InkPoc.Services.Ink;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class InkNodeSelectionManager
    {
        const double BUSY_WAITING_TIME = 200;
        const double TRIPLE_TAP_TIME = 400;

        private readonly InkCanvas inkCanvas;
        private readonly InkPresenter inkPresenter;
        private readonly InkAsyncAnalyzer analyzer;

        private readonly InkStrokesService strokeService;
        private readonly InkSelectionRectangleManager selectionRectangleManager;

        IInkAnalysisNode selectedNode;
        private readonly Canvas selectionCanvas;

        DateTime lastDoubleTapTime;

        public InkNodeSelectionManager(InkCanvas _inkCanvas, Canvas _selectionCanvas, InkAsyncAnalyzer _analyzer, InkStrokesService _strokeService, InkSelectionRectangleManager _selectionRectangleManager)
        {
            // Initialize properties
            inkCanvas = _inkCanvas;
            selectionCanvas = _selectionCanvas;
            inkPresenter = inkCanvas.InkPresenter;
            analyzer = _analyzer;
            strokeService = _strokeService;
            selectionRectangleManager = _selectionRectangleManager;

            // selection on tap
            inkCanvas.Tapped += InkCanvas_Tapped;
            inkCanvas.DoubleTapped += InkCanvas_DoubleTapped;

            //drag and drop
            inkCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            inkCanvas.PointerPressed += InkCanvas_PointerPressed;
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
                selectedNode = analyzer.FindHitNode(position);
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

            if (selectionRectangleManager.ContainsPosition(position))
            {
                // Pressed on the selected rect, do nothing
                return;
            }

            selectedNode = analyzer.FindHitNode(position);
            ShowOrHideSelection(selectedNode);
        }

        public void ClearSelection()
        {
            selectedNode = null;
            selectionRectangleManager.Clear();
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
            if (node != null)
            {
                var rect = strokeService.SelectStrokesByNode(node);
                selectionRectangleManager.UpdateSelectionRect(rect);
            }
            else
            {
                ClearSelection();
            }
        }

    }
}
