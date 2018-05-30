using System;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;

namespace InkPoc.Helpers.Ink
{
    public class InkAsyncAnalyzer
    {
        private readonly InkStrokeContainer strokeContainer;
        private readonly DispatcherTimer dispatcherTimer;
        const double IDLE_WAITING_TIME = 400;
        
        public InkAsyncAnalyzer(InkStrokeContainer strokeContainer)
        {
            this.strokeContainer = strokeContainer;

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
                InkAnalyzer.AddDataForStrokes(strokeContainer.GetStrokes());
            }

            var result = await InkAnalyzer.AnalyzeAsync();
            return result.Status == InkAnalysisStatus.Updated;
        }

        public void StartTimer() => dispatcherTimer.Start();

        public void StopTimer() => dispatcherTimer.Stop();

        private async void DispatcherTimer_Tick(object sender, object e) => await AnalyzeAsync();
                
    }
}
