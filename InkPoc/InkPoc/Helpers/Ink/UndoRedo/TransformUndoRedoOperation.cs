using InkPoc.Services.Ink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class TransformUndoRedoOperation : IUndoRedoOperation
    {
        private Canvas drawingCanvas;
        private InkTransformResult transformResult;
        private readonly InkStrokesService strokeService;

        public TransformUndoRedoOperation(InkTransformResult _transformResult, Canvas _drawingCanvas, InkStrokesService _strokeService)
        {
            transformResult = _transformResult;
            drawingCanvas = _drawingCanvas;
            strokeService = _strokeService;

            strokeService.AddStrokeEvent += StrokeService_AddStrokeEvent;
        }

        public void ExecuteRedo()
        {
            //convertir las strokes en shapes
            RemoveStrokes();
            AddTextAndShapes();
        }

        public void ExecuteUndo()
        {
            //convertir las shapes en strokes
            RemoveTextAndShapes();
            AddStrokes();
        }

        private void StrokeService_AddStrokeEvent(object sender, AddStrokeToContainerEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var removedStrokes = transformResult.Strokes.RemoveAll(s => s.Id == e.OldStroke?.Id);
            if (removedStrokes > 0)
            {
                transformResult.Strokes.Add(e.NewStroke);
            }
        }

        private void AddTextAndShapes()
        {
            foreach (var uielement in transformResult.TextAndShapes.ToList())
            {
                drawingCanvas.Children.Add(uielement);
            }
        }

        private void RemoveTextAndShapes()
        {
            foreach (var uielement in transformResult.TextAndShapes)
            {
                if (drawingCanvas.Children.Contains(uielement))
                {
                    drawingCanvas.Children.Remove(uielement);
                }
            }
        }

        private void AddStrokes()
        {
            foreach (var stroke in transformResult.Strokes.ToList())
            {
                strokeService.AddStrokeToContainer(stroke);
            }
        }

        private void RemoveStrokes()
        {
            foreach (var stroke in transformResult.Strokes)
            {
                strokeService.RemoveStrokeToContainer(stroke);
            }
        }
    }
}
