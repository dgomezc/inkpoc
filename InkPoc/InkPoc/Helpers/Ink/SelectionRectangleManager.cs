﻿using InkPoc.Services.Ink;
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



    public class SelectionRectangleManager
    {
        Point dragStartPosition;
        private readonly Canvas selectionCanvas;
        InkCanvas inkCanvas;
        Rect selectionStrokesRect = Rect.Empty;
        private readonly InkStrokesService strokeService;

        public SelectionRectangleManager(InkCanvas _inkCanvas, Canvas _selectionCanvas, InkStrokesService _strokeService)
        {
            selectionCanvas = _selectionCanvas;
            inkCanvas = _inkCanvas;
            strokeService = _strokeService;

            inkCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            //inkCanvas.PointerPressed += InkCanvas_PointerPressed;
            inkCanvas.ManipulationStarted += InkCanvas_ManipulationStarted;
            inkCanvas.ManipulationDelta += InkCanvas_ManipulationDelta;
            inkCanvas.ManipulationCompleted += InkCanvas_ManipulationCompleted;
        }

        public void UpdateSelectionRect(Rect rect)
        {
            selectionStrokesRect = rect;

            var selectionRect = GetSelectionRectangle();

            selectionRect.Width = rect.Width;
            selectionRect.Height = rect.Height;
            Canvas.SetLeft(selectionRect, rect.Left);
            Canvas.SetTop(selectionRect, rect.Top);
        }

        public void Clear()
        {
            selectionStrokesRect = Rect.Empty;
            selectionCanvas.Children.Clear();
        }

        public bool ContainsPosition(Point position)
        {
            return !selectionStrokesRect.IsEmpty && RectHelper.Contains(selectionStrokesRect, position);
        }

        private Rectangle GetSelectionRectangle()
        {
            var selectionRectange = selectionCanvas.Children.FirstOrDefault(f => f is Rectangle r && r.Name == "selectionRectangle") as Rectangle;

            if (selectionRectange == null)
            {
                selectionRectange = CreateNewSelectionRectangle();
                selectionCanvas.Children.Add(selectionRectange);
            }

            return selectionRectange;
        }

        private Rectangle CreateNewSelectionRectangle()
        {
            return new Rectangle()
            {
                Name = "selectionRectangle",
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection() { 2, 2 },
                StrokeDashCap = PenLineCap.Round
            };
        }






        ////private async void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        ////{
        ////    var position = e.GetCurrentPoint(inkCanvas).Position;

        ////    if (!selectionStrokesRect.IsEmpty && RectHelper.Contains(selectionStrokesRect, position))
        ////    {
        ////        // Pressed on the selected rect, do nothing
        ////        return;
        ////    }

        ////    selectedNode = analyzer.FindHitNode(position);
        ////    ShowOrHideSelection(selectedNode);
        ////}

        private void InkCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!selectionStrokesRect.IsEmpty)
            {
                dragStartPosition = e.Position;
            }
        }

        private void InkCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!selectionStrokesRect.IsEmpty)
            {
                Point offset;
                offset.X = e.Delta.Translation.X;
                offset.Y = e.Delta.Translation.Y;
                MoveSelection(offset);
            }
        }

        private void InkCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!selectionStrokesRect.IsEmpty)
            {
                strokeService.MoveSelectedStrokes(dragStartPosition, e.Position);
            }
        }

        private void MoveSelection(Point offset)
        {
            var selectionRect = GetSelectionRectangle();

            var left = Canvas.GetLeft(selectionRect);
            var top = Canvas.GetTop(selectionRect);
            Canvas.SetLeft(selectionRect, left + offset.X);
            Canvas.SetTop(selectionRect, top + offset.Y);

            selectionStrokesRect.X = left + offset.X;
            selectionStrokesRect.Y = top + offset.Y;
        }
    }
}