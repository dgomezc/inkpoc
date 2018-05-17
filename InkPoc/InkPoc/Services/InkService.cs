﻿using Microsoft.Graphics.Canvas;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace InkPoc.Services
{
    public static class InkService
    {
        public static async Task<string> RecognizeTextAsync(InkStrokeContainer container)
        {
            if (!container.GetStrokes().Any())
            {
                return string.Empty;
            }

            var recognizer = new InkRecognizerContainer();
            var candidates = await recognizer.RecognizeAsync(container, InkRecognitionTarget.All);

            var result = candidates.Select(x => x.GetTextCandidates().FirstOrDefault())
                .Where(x => !string.IsNullOrEmpty(x));

            return string.Join(" ", result);
        }
                
        public static async Task LoadInkAsync(InkStrokeContainer container)
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".gif");

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
            if (!container.GetStrokes().Any())
            {
                return;
            }

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Gif with embedded ISF", new List<string> { ".gif" });

            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                // Prevent updates to the file until updates are finalized with call to CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await container.SaveAsync(stream);
                }

                // Finalize write so other apps can update file.
                var status = await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        public static async Task ExportToImageAsync(InkStrokeContainer container, Size canvasSize, StorageFile imageFile = null)
        {
            if(!container.GetStrokes().Any())
            {
                return;
            }

            if (imageFile != null)
            {
                await ExportCanvasAndImageAsync(container, canvasSize, imageFile);
            }
            else
            {
                await ExportCanvasAsync(container, canvasSize);
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

        private static async Task ExportCanvasAndImageAsync(InkStrokeContainer container, Size canvasSize, StorageFile imageFile)
        {
            var saveFile = await GetImageToSaveAsync();

            if (saveFile == null)
            {
                return;
            }

            // Prevent updates to the file until updates are finalized with call to CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(saveFile);

            using (var outStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var device = CanvasDevice.GetSharedDevice();
                
                CanvasBitmap canvasbitmap;
                using (var stream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    canvasbitmap = await CanvasBitmap.LoadAsync(device, stream);
                }

                using (var renderTarget = new CanvasRenderTarget(device, (int)canvasSize.Width, (int)canvasSize.Height, canvasbitmap.Dpi))
                {
                    using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
                    {
                        ds.Clear(Colors.White);

                        ds.DrawImage(canvasbitmap, new Rect(0, 0, (int)canvasSize.Width, (int)canvasSize.Height));
                        ds.DrawInk(container.GetStrokes());
                    }

                    await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Png);
                }
            }

            // Finalize write so other apps can update file.
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(saveFile);
        }

        private static async Task ExportCanvasAsync(InkStrokeContainer container, Size canvasSize)
        {
            var file = await GetImageToSaveAsync();
            if(file == null)
            {
                return;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)canvasSize.Width, (int)canvasSize.Height, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawInk(container.GetStrokes());
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
            }
        }

        private static async Task<StorageFile> GetImageToSaveAsync()
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            var saveFile = await savePicker.PickSaveFileAsync();

            return saveFile;
        }
    }
}
