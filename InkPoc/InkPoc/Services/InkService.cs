using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        public static async Task LoadFileAsync(InkStrokeContainer container)
        {
            var file = await GetFileAsync();

            if (file is null)
            {
                return;
            }

            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    await container.LoadAsync(inputStream);
                }
            }
        }

        public static async Task SaveFileAsync(InkStrokeContainer container)
        {
            if(!container.GetStrokes().Any())
            {
                return;
            }

            var file = await SaveFileAsync();

            if(file is null)
            {
                return;
            }

            // Prevent updates to the file until updates are finalized with call to CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(file);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    await container.SaveAsync(outputStream);
                    await outputStream.FlushAsync();
                }
            }

            ////// Finalize write so other apps can update file.
            ////Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

            ////if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
            ////{
            ////    // File saved.
            ////}
            ////else
            ////{
            ////    // File couldn't be saved.
            ////}
        }

        private static async Task<StorageFile> GetFileAsync()
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".gif");

            // Show the file picker.
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }

        private static async Task<StorageFile> SaveFileAsync()
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("GIF with embedded ISF", new List<string>() { ".gif" });
            savePicker.DefaultFileExtension = ".gif";
            savePicker.SuggestedFileName = "InkSample";

            var file = await savePicker.PickSaveFileAsync();
            return file;
        }
    }
}
