using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
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

        public static async Task LoadInkAsync(InkStrokeContainer container)
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".isf");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                {
                    await container.LoadAsync(stream);
                }
            }            
        }

        public static async Task SaveInkAsync(InkStrokeContainer container)
        {
            if (container.GetStrokes().Any())
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("Gif with embedded ISF", new List<string> { ".gif" });

                var file = await savePicker.PickSaveFileAsync();

                if (null != file)
                {
                    // Prevent updates to the file until updates are finalized with call to CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await container.SaveAsync(stream);
                    }
                }

                // Finalize write so other apps can update file.
                var status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == FileUpdateStatus.Complete)
                {
                    // File saved.
                }
                else
                {
                    // File couldn't be saved.
                }
            }
        }

        public static void ClearStrokesSelection(InkStrokeContainer container)
        {
            var strokes = container.GetStrokes();
            foreach (var stroke in strokes)
            {
                stroke.Selected = false;
            }
        }
    }
}
