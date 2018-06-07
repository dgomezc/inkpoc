using InkPoc.Services.Ink;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;

namespace InkPoc.Helpers.Ink
{
    public class InkAsyncAnalyzer
    {
        private readonly InkStrokesService strokesService;
        private readonly DispatcherTimer dispatcherTimer;
        const double IDLE_WAITING_TIME = 400;
        
        public InkAsyncAnalyzer(InkStrokesService _strokesService)
        {
            strokesService = _strokesService;
            strokesService.AddStrokeEvent += StrokesService_AddStrokeEvent;
            strokesService.RemoveStrokeEvent += StrokesService_RemoveStrokeEvent;
            strokesService.MoveStrokesEvent += StrokesService_MoveStrokesEvent;
            strokesService.PasteStrokesEvent += StrokesService_PasteStrokesEvent;


            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(IDLE_WAITING_TIME);
        }        

        public InkAnalyzer InkAnalyzer { get; set; } = new InkAnalyzer();

        public bool IsAnalyzing => InkAnalyzer.IsAnalyzing;

        public async Task<bool> AnalyzeAsync(bool clean = false)
        {
            StopTimer();

            if (IsAnalyzing)
            {
                // Ink analyzer is busy. Wait a while and try again.
                StartTimer();
                return false;
            }

            if (clean == true)
            {
                InkAnalyzer.ClearDataForAllStrokes();
                InkAnalyzer.AddDataForStrokes(strokesService.GetStrokes());
            }

            var result = await InkAnalyzer.AnalyzeAsync();
            return result.Status == InkAnalysisStatus.Updated;
        }

        public IInkAnalysisNode FindHitNode(Point position)
        {
            // Start with smallest scope
            var node = FindHitNodeByKind(position, InkAnalysisNodeKind.InkWord);
            if (node == null)
            {
                node = FindHitNodeByKind(position, InkAnalysisNodeKind.InkBullet);
                if (node == null)
                {
                    node = FindHitNodeByKind(position, InkAnalysisNodeKind.InkDrawing);
                }
            }
            return node;
        }

        public void StartTimer() => dispatcherTimer.Start();

        public void StopTimer() => dispatcherTimer.Stop();

        public void ClearAnalysis()
        {
            StopTimer();
            InkAnalyzer.ClearDataForAllStrokes();
        }

        public void AddStroke(InkStroke stroke)
        {
            StopTimer();
            InkAnalyzer.AddDataForStroke(stroke);
            StartTimer();
        }

        public void AddStrokes(IReadOnlyList<InkStroke> strokes)
        {
            StopTimer();
            InkAnalyzer.AddDataForStrokes(strokes);
            StartTimer();
        }

        public void RemoveStroke(InkStroke stroke)
        {
            StopTimer();
            InkAnalyzer.RemoveDataForStroke(stroke.Id);
            StartTimer();
        }

        public void RemoveStrokes(IReadOnlyList<InkStroke> strokes)
        {
            StopTimer();

            foreach (var stroke in strokes)
            {
                // Remove strokes from InkAnalyzer
                InkAnalyzer.RemoveDataForStroke(stroke.Id);
            }
            StartTimer();
        }

        public void ReplaceStroke(InkStroke stroke)
        {
            InkAnalyzer.ReplaceDataForStroke(stroke);
        }

        private async void DispatcherTimer_Tick(object sender, object e) => await AnalyzeAsync();

        private IInkAnalysisNode FindHitNodeByKind(Point position, InkAnalysisNodeKind kind)
        {
            var nodes = InkAnalyzer.AnalysisRoot.FindNodes(kind);
            foreach (var node in nodes)
            {
                if (RectHelper.Contains(node.BoundingRect, position))
                {
                    return node;
                }
            }
            return null;
        }

        private void StrokesService_AddStrokeEvent(object sender, AddStrokeToContainerEventArgs e)
        {
            AddStroke(e.NewStroke);
        }

        private void StrokesService_RemoveStrokeEvent(object sender, RemoveStrokeToContainerEventArgs e)
        {
            RemoveStroke(e.RemovedStroke);
        }

        private async void StrokesService_MoveStrokesEvent(object sender, MoveStrokesEventArgs e)
        {
            foreach (var stroke in e.Strokes)
            {
                ReplaceStroke(stroke);
            }

            // Strokes are moved and the analysis result is not valid anymore.
            await AnalyzeAsync(true);
        }

        private async void StrokesService_PasteStrokesEvent(object sender, CopyPasteStrokesEventArgs e)
        {
            //Strokes are paste and the analysis result is not valid anymore.
            await AnalyzeAsync(true);
        }
    }
}
