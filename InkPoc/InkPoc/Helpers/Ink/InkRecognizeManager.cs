using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Helpers.Ink
{
    public class InkRecognizeManager
    {
        private InkStrokeContainer _container;
        private Canvas _drawingCanvas;
        private InkAnalyzer _inkAnalyzer;

        public InkRecognizeManager(InkPresenter inkPresenter, Canvas drawingCanvas)
        {
            _container = inkPresenter.StrokeContainer;
            _drawingCanvas = drawingCanvas;
            _inkAnalyzer = new InkAnalyzer();
        }

        public async Task AnalyzeStrokesAsync()
        {
            var inkStrokes = _container.GetStrokes();

            if (!inkStrokes.Any())
            {
                return;
            }

            _inkAnalyzer.AddDataForStrokes(inkStrokes);

            //foreach (var stroke in inkStrokes)
            //{
            //    _inkAnalyzer.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
            //}

            var inkAnalysisResults = await _inkAnalyzer.AnalyzeAsync();

            if (inkAnalysisResults.Status == InkAnalysisStatus.Updated)
            {
                AnalyzeWords();
                AnalyzeShapes();
            }
        }

        // TODO: Merge with Inkservice -> RecognizeTextAsync
        public async Task AnalyzeTextAsync()
        {
            if (!_container.GetStrokes().Any())
            {
                return;
            }

            var recognizer = new InkRecognizerContainer();
            var candidates = await recognizer.RecognizeAsync(_container, InkRecognitionTarget.All);

            foreach(var candidate in candidates)
            {
                var text = candidate
                            .GetTextCandidates()
                            .FirstOrDefault(t => !string.IsNullOrEmpty(t));

                DrawText(text, candidate.BoundingRect);

                foreach (var stroke in candidate.GetStrokes())
                {
                    stroke.Selected = true;
                }
            }
            _container.DeleteSelected();
        }

        public void ClearAnalyzer()
        {
            _inkAnalyzer.ClearDataForAllStrokes();
        }

        private void AnalyzeWords()
        {
            var inkwordNodes = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkWord);
            foreach (InkAnalysisInkWord node in inkwordNodes)
            {
                DrawText(node.RecognizedText, node.BoundingRect);

                foreach (var strokeId in node.GetStrokeIds())
                {
                    var stroke = _container.GetStrokeById(strokeId);
                    stroke.Selected = true;
                }
                _inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
            }
            _container.DeleteSelected();
        }

        private void AnalyzeShapes()
        {
            var inkdrawingNodes = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing);
            foreach (InkAnalysisInkDrawing node in inkdrawingNodes)
            {
                if (node.DrawingKind == InkAnalysisDrawingKind.Drawing)
                {
                    // Catch and process unsupported shapes (lines and so on) here.
                }
                else
                {
                    if (node.DrawingKind == InkAnalysisDrawingKind.Circle || node.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                    {
                        DrawEllipse(node);
                    }
                    else
                    {
                        DrawPolygon(node);
                    }

                    foreach (var strokeId in node.GetStrokeIds())
                    {
                        var stroke = _container.GetStrokeById(strokeId);
                        stroke.Selected = true;
                    }
                }
                _inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
            }
            _container.DeleteSelected();
        }

        private void DrawText(string recognizedText, Rect boundingRect)
        {
            TextBlock text = new TextBlock();
            Canvas.SetTop(text, boundingRect.Top);
            Canvas.SetLeft(text, boundingRect.Left);

            text.Text = recognizedText;
            text.FontSize = boundingRect.Height;

            _drawingCanvas.Children.Add(text);
        }

        private void DrawEllipse(InkAnalysisInkDrawing shape)
        {
            var points = shape.Points;
            Ellipse ellipse = new Ellipse();
            ellipse.Width = Math.Sqrt((points[0].X - points[2].X) * (points[0].X - points[2].X) +
                 (points[0].Y - points[2].Y) * (points[0].Y - points[2].Y));
            ellipse.Height = Math.Sqrt((points[1].X - points[3].X) * (points[1].X - points[3].X) +
                 (points[1].Y - points[3].Y) * (points[1].Y - points[3].Y));

            var rotAngle = Math.Atan2(points[2].Y - points[0].Y, points[2].X - points[0].X);
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = rotAngle * 180 / Math.PI;
            rotateTransform.CenterX = ellipse.Width / 2.0;
            rotateTransform.CenterY = ellipse.Height / 2.0;

            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = shape.Center.X - ellipse.Width / 2.0;
            translateTransform.Y = shape.Center.Y - ellipse.Height / 2.0;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);
            ellipse.RenderTransform = transformGroup;

            var brush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 255));
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 2;
            _drawingCanvas.Children.Add(ellipse);
        }

        private void DrawPolygon(InkAnalysisInkDrawing shape)
        {
            var points = shape.Points;
            Polygon polygon = new Polygon();

            foreach (var point in points)
            {
                polygon.Points.Add(point);
            }

            var brush = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 0, 0, 255));
            polygon.Stroke = brush;
            polygon.StrokeThickness = 2;
            _drawingCanvas.Children.Add(polygon);
        }
    }
}
