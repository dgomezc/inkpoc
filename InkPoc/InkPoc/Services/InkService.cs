using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;

namespace InkPoc.Services
{
    public static class InkService
    {
        public static async Task<string> RecognizeTextOneAsync(InkStrokeContainer container)
        {
            if (container.GetStrokes().Any())
            {
                var recognizer = new InkRecognizerContainer();

                var candidates = await recognizer.RecognizeAsync(container, InkRecognitionTarget.All);

                var result = candidates.Select(x => x.GetTextCandidates().FirstOrDefault())
                    .Where(x => !string.IsNullOrEmpty(x));

                return string.Join(" ", result);
            }

            return string.Empty;
        }

        public static async Task<string> RecognizeTextTwoAsync(InkStrokeContainer container)
        {
            var textResult = string.Empty;
            var strokesText = container.GetStrokes();
            // Ensure an ink stroke is present.
            if (strokesText.Count > 0)
            {
                var analyzerText = new InkAnalyzer();
                analyzerText.AddDataForStrokes(strokesText);

                // Force analyzer to process strokes as handwriting.
                foreach (var stroke in strokesText)
                {
                    analyzerText.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
                }

                var resultText = await analyzerText.AnalyzeAsync();

                if (resultText.Status == InkAnalysisStatus.Updated)
                {
                    textResult = analyzerText.AnalysisRoot.RecognizedText;
                    //var words = analyzerText.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkWord);
                    //foreach (var word in words)
                    //{
                    //    InkAnalysisInkWord concreteWord = (InkAnalysisInkWord)word;
                    //    foreach (string s in concreteWord.TextAlternates)
                    //    {
                    //        textResult += s;
                    //    }
                    //}
                }
                analyzerText.ClearDataForAllStrokes();
            }

            return textResult;
        }
    }
}
