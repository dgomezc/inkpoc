using System;
using System.Threading.Tasks;
using InkPoc.Helpers.Ink;
using InkPoc.ViewModels;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Views
{
    public sealed partial class TextSelectionPage : Page
    {
        public TextSelectionViewModel ViewModel { get; } = new TextSelectionViewModel();

        DispatcherTimer dispatcherTimer;
        const double IDLE_WAITING_TIME = 400;
        const double TRIPLE_TAP_TIME = 400;
        const double BUSY_WAITING_TIME = 200;

        InkPresenter inkPresenter;
        InkStrokeContainer strokeContainer;
        InkAnalyzer inkAnalyzer;
        Point dragStartPosition;

        IInkAnalysisNode selectedNode;
        DateTime lastDoubleTapTime;

        Polyline lasso;
        bool isBoundRect = false;

        public TextSelectionPage()
        {
            InitializeComponent();

            inkCanvas.Tapped += InkCanvas_Tapped;
            inkCanvas.DoubleTapped += InkCanvas_DoubleTapped;
            inkCanvas.PointerPressed += InkCanvas_PointerPressed;
            inkCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            inkCanvas.ManipulationStarted += InkCanvas_ManipulationStarted;
            inkCanvas.ManipulationDelta += InkCanvas_ManipulationDelta;
            inkCanvas.ManipulationCompleted += InkCanvas_ManipulationCompleted;
            
            inkPresenter = inkCanvas.InkPresenter;
            inkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkPresenter.StrokesErased += InkPresenter_StrokesErased;

            strokeContainer = inkPresenter.StrokeContainer;
            inkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
            

            inkAnalyzer = new InkAnalyzer();

            // Perform analysis when there has been a change to the ink presenter and 
            // the user hasn't written a new stroke for IDLE_WAITING_TIME
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick; ;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(IDLE_WAITING_TIME);
        }

        private void InkCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var position = e.GetPosition(inkCanvas);
            if (selectedNode != null && RectHelper.Contains(InkHelper.GetCurrentBoundingRectOfNode(strokeContainer, selectedNode), position))
            {
                if (DateTime.Now.Subtract(lastDoubleTapTime).TotalMilliseconds < TRIPLE_TAP_TIME)
                {
                    ExpandSelection();
                }
            }
            else
            {
                selectedNode = InkHelper.FindHitNode(ref inkAnalyzer, position, strokeContainer);
                ShowOrHideSelection(selectedNode, true);
            }
        }

        private void InkCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(inkCanvas);
            if (selectedNode != null &&
                RectHelper.Contains(InkHelper.GetCurrentBoundingRectOfNode(strokeContainer, selectedNode), position))
            {
                ExpandSelection();
                lastDoubleTapTime = System.DateTime.Now;
            }
        }
        
        private async void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(inkCanvas).Position;
            while (inkAnalyzer.IsAnalyzing)
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(BUSY_WAITING_TIME));
            }

            if ((selectedNode != null) &&
               (RectHelper.Contains(InkHelper.GetCurrentBoundingRectOfNode(strokeContainer, selectedNode), position)))
            {
                // Pressed on the selected node, do nothing
                return;
            }

            selectedNode = InkHelper.FindHitNode(ref inkAnalyzer, position, strokeContainer);
            ShowOrHideSelection(selectedNode, true);
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
                var mx = InkHelper.GetTranslationMX(e.Position.X - dragStartPosition.X, e.Position.Y - dragStartPosition.Y);
                InkHelper.TransformInk(strokeContainer, selectedNode, mx);
                InkHelper.UpdateInkForNode(inkAnalyzer, strokeContainer, selectedNode);

                // Strokes are moved and the analysis result is not valid anymore.
                await AnalyzeInk();
            }
        }
        
        private void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            // Don't perform analysis while user is inking
            dispatcherTimer.Stop();

            // Quit lasso selection state
            //lassoSelectionToggleButton.IsChecked = false;
            inkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            dispatcherTimer.Stop();
            foreach (var stroke in args.Strokes)
            {
                // Remove strokes from InkAnalyzer
                inkAnalyzer.RemoveDataForStroke(stroke.Id);
            }
            dispatcherTimer.Start();

            // Quit lasso selection state
            //lassoSelectionToggleButton.IsChecked = false;
            inkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            dispatcherTimer.Stop();
            inkAnalyzer.AddDataForStrokes(args.Strokes);
            dispatcherTimer.Start();
        }
               
        private void SelectionButton_Checked(object sender, RoutedEventArgs e)
        {
            inkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
        }

        private void SelectionButton_Unchecked(object sender, RoutedEventArgs e)
        {
            inkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
        }
        
        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso = new Polyline()
            {
                Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
            };

            lasso.Points.Add(args.CurrentPoint.RawPosition);
            selectionCanvas.Children.Add(lasso);
            isBoundRect = true;
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            if (isBoundRect)
            {
                lasso.Points.Add(args.CurrentPoint.RawPosition);
            }
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);

            var rect = strokeContainer.SelectWithPolyLine(lasso.Points);
            isBoundRect = false;

            selectionCanvas.Children.Remove(lasso);
            UpdateSelection(rect);
        }

        private void UpdateSelection(Rect rect)
        {
            selectionRect.Width = rect.Width;
            selectionRect.Height = rect.Height;
            Canvas.SetLeft(selectionRect, rect.Left);
            Canvas.SetTop(selectionRect, rect.Top);
            selectionRect.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async Task<bool> AnalyzeInk(bool clean = false)
        {
            bool updated = false;
            dispatcherTimer.Stop();
            if (inkAnalyzer.IsAnalyzing)
            {
                // Ink analyzer is busy. Wait a while and try again.
                dispatcherTimer.Start();
            }
            else
            {
                if (clean == true)
                {
                    inkAnalyzer.ClearDataForAllStrokes();
                    inkAnalyzer.AddDataForStrokes(strokeContainer.GetStrokes());
                }
                var ret = await inkAnalyzer.AnalyzeAsync();
                if (ret.Status == InkAnalysisStatus.Updated)
                {
                    updated = true;
                }
            }
            return updated;
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            await AnalyzeInk();
        }

        private void MouseInkButton_Checked(object sender, RoutedEventArgs e)
        {
            inkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

        }

        private void MouseInkButton_Unchecked(object sender, RoutedEventArgs e)
        {
            inkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();

            strokeContainer.Clear();
            inkAnalyzer.ClearDataForAllStrokes();

            SetSelectionVisual(false);
            selectedNode = null;

            selectionCanvas.Children.Clear();
            selectionCanvas.Children.Add(selectionRect);
        }

        private void MoveSelection(Point offset)
        {
            var left = Canvas.GetLeft(selectionRect);
            var top = Canvas.GetTop(selectionRect);
            Canvas.SetLeft(selectionRect, left + offset.X);
            Canvas.SetTop(selectionRect, top + offset.Y);
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
                ShowOrHideSelection(selectedNode, true);
            }
        }

        private void ShowOrHideSelection(IInkAnalysisNode node, bool considerStyling = false)
        {
            if (node == null)
            {
                SetSelectionVisual(false);
            }
            else
            {
                var rect = node.BoundingRect;
                if (considerStyling)
                {
                    rect = InkHelper.GetCurrentBoundingRectOfNode(strokeContainer, node);
                }
                UpdateSelection(rect);
            }
        }

        private void SetSelectionVisual(bool show)
        {
            if (show)
            {
                selectionRect.Visibility = Visibility.Visible;
            }
            else
            {
                selectionRect.Visibility = Visibility.Collapsed;
            }
        }
    }
}
