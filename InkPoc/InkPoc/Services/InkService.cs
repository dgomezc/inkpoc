using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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

        public static async Task ExportAsImageAsync(InkCanvas inkCanvas)
        {
            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Any())
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });

                var saveFile = await savePicker.PickSaveFileAsync();
                //await Save_InkedImagetoFile(saveFile);
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

        //private static async Task Save_InkedImagetoFile(StorageFile saveFile)
        //{
        //    if (saveFile != null)
        //    {
        //        CachedFileManager.DeferUpdates(saveFile);

        //        using (var outStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
        //        {
        //            await Save_InkedImagetoStream(outStream);
        //        }

        //        var status =  await CachedFileManager.CompleteUpdatesAsync(saveFile);
        //    }
        //}

        //private static async Task Save_InkedImagetoStream(IRandomAccessStream stream)
        //{
        //    var file = await StorageFile.GetFileFromApplicationUriAsync(((BitmapImage)myImage.Source).UriSource);

        //    var device = CanvasDevice.GetSharedDevice();

        //    var image = await CanvasBitmap.LoadAsync(device, file.Path);

        //    using (var renderTarget = new CanvasRenderTarget(device, (int)myInkCanvas.ActualWidth, (int)myInkCanvas.ActualHeight, image.Dpi))
        //    {
        //        using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
        //        {
        //            ds.Clear(Colors.White);

        //            ds.DrawImage(image, new Rect(0, 0, (int)myInkCanvas.ActualWidth, (int)myInkCanvas.ActualHeight));
        //            ds.DrawInk(myInkCanvas.InkPresenter.StrokeContainer.GetStrokes());
        //        }

        //        await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        //    }
        //}

        private static async Task ExportInkCanvasToFile(InkCanvas inkCanvas, StorageFile file)
        {            
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
            }
        }
    }
}
