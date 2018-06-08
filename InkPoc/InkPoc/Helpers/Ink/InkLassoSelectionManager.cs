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
    public class InkLassoSelectionManager
    {
        private readonly InkPresenter inkPresenter;
        private readonly Canvas selectionCanvas;
        private readonly InkStrokesService strokeService;
        private readonly InkSelectionRectangleManager selectionRectangleManager;

        private bool enableLasso;
        private Polyline lasso;        

        public InkLassoSelectionManager(InkCanvas _inkCanvas, Canvas _selectionCanvas, InkStrokesService _strokeService, InkSelectionRectangleManager _selectionRectangleManager)
        {
            // Initialize properties
            inkPresenter = _inkCanvas.InkPresenter;
            selectionCanvas = _selectionCanvas;
            strokeService = _strokeService;
            selectionRectangleManager = _selectionRectangleManager;

            // lasso selection
            inkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            EndLassoSelectionConfig();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            EndLassoSelectionConfig();
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

            var rect = strokeService.SelectStrokesByPoints(lasso.Points);
            enableLasso = false;

            selectionCanvas.Children.Remove(lasso);
            selectionRectangleManager.UpdateSelectionRect(rect);
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

        public void ClearSelection()
        {
            selectionRectangleManager.Clear();
        }

    }
}
