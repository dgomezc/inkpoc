using InkPoc.Services;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Helpers.Ink
{
    public class InkSelectionManager
    {
        private InkPresenter _inkPresenter;
        private Canvas _selectionCanvas;
        private Rect boundingRect;
        private Polyline lasso;

        public InkSelectionManager(InkPresenter inkPresenter, Canvas selectionCanvas)
        {
            _inkPresenter = inkPresenter;
            _selectionCanvas = selectionCanvas;

            // Listen for new ink or erase strokes to clean up selection UI.
            _inkPresenter.StrokeInput.StrokeStarted += (s,e) => ClearSelection();
            _inkPresenter.StrokesErased += (s, e) => ClearSelection();
        }

        public void ClearSelection()
        {
            EndSelection();
            InkService.ClearStrokesSelection(_inkPresenter.StrokeContainer);

            if (_selectionCanvas.Children.Any())
            {
                _selectionCanvas.Children.Clear();
                boundingRect = Rect.Empty;
            }
        }

        public void StartSelection()
        {
            // By default, the InkPresenter processes input modified by 
            // a secondary affordance (pen barrel button, right mouse 
            // button, or similar) as ink.
            // To pass through modified input to the app for custom processing 
            // on the app UI thread instead of the background ink thread, set 
            // InputProcessingConfiguration.RightDragAction to LeaveUnprocessed.
            _inkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            // Listen for unprocessed pointer events from modified input.
            // The input is used to provide selection functionality.
            _inkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            _inkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            _inkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
        }

        public void EndSelection()
        {
            _inkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            _inkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            _inkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
        }

        // Handle unprocessed pointer events from modifed input.
        // The input is used to provide selection functionality.
        // Selection UI is drawn on a canvas under the InkCanvas.
        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            // Initialize a selection lasso.
            lasso = new Polyline()
            {
                Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
            };

            lasso.Points.Add(args.CurrentPoint.RawPosition);

            _selectionCanvas.Children.Add(lasso);
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            // Add a point to the lasso Polyline object.
            lasso.Points.Add(args.CurrentPoint.RawPosition);
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            // Add the final point to the Polyline object and 
            // select strokes within the lasso area.
            // Draw a bounding box on the selection canvas 
            // around the selected ink strokes.
            lasso.Points.Add(args.CurrentPoint.RawPosition);

            boundingRect = _inkPresenter.StrokeContainer.SelectWithPolyLine(lasso.Points);

            DrawBoundingRect();
        }

        // Draw a bounding rectangle, on the selection canvas, encompassing 
        // all ink strokes within the lasso area.
        private void DrawBoundingRect()
        {
            // Clear all existing content from the selection canvas.
            _selectionCanvas.Children.Clear();

            // Draw a bounding rectangle only if there are ink strokes 
            // within the lasso area.
            if (!((boundingRect.Width == 0) ||
                (boundingRect.Height == 0) ||
                boundingRect.IsEmpty))
            {
                var rectangle = new Rectangle()
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 5, 2 },
                    Width = boundingRect.Width,
                    Height = boundingRect.Height
                };

                Canvas.SetLeft(rectangle, boundingRect.X);
                Canvas.SetTop(rectangle, boundingRect.Y);

                _selectionCanvas.Children.Add(rectangle);
            }
        }
    }
}
