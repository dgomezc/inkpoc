﻿using InkPoc.Services.Ink;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Helpers.Ink
{
    public class InkFileManager
    {
        private readonly InkStrokesService strokesService;
        private readonly InkCanvas inkCanvas;

        public InkFileManager(InkCanvas _inkCanvas, InkStrokesService _strokesService)
        {
            inkCanvas = _inkCanvas;
            strokesService = _strokesService;
        }

        public async Task LoadInkAsync()
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".gif");

            var file = await openPicker.PickSingleFileAsync();
            await strokesService.LoadInkFileAsync(file);
        }

        public async Task SaveInkAsync()
        {
            if (!strokesService.GetStrokes().Any())
            {
                return;
            }

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Gif with embedded ISF", new List<string> { ".gif" });

            var file = await savePicker.PickSaveFileAsync();
            await strokesService.SaveInkFileAsync(file);
        }

        public async Task ExportToImageAsync(StorageFile imageFile = null)
        {
            if (!strokesService.GetStrokes().Any())
            {
                return;
            }

            if (imageFile != null)
            {
                await ExportCanvasAndImageAsync(imageFile);
            }
            else
            {
                await ExportCanvasAsync();
            }
        }

        private  async Task ExportCanvasAndImageAsync(StorageFile imageFile)
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

                using (var renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.Width, (int)inkCanvas.Height, canvasbitmap.Dpi))
                {
                    using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
                    {
                        ds.Clear(Colors.White);

                        ds.DrawImage(canvasbitmap, new Rect(0, 0, (int)inkCanvas.Width, (int)inkCanvas.Height));
                        ds.DrawInk(strokesService.GetStrokes());
                    }

                    await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Png);
                }
            }

            // Finalize write so other apps can update file.
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(saveFile);
        }

        private async Task ExportCanvasAsync()
        {
            var file = await GetImageToSaveAsync();
            if (file == null)
            {
                return;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.Width, (int)inkCanvas.Height, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawInk(strokesService.GetStrokes());
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
            }
        }

        private async Task<StorageFile> GetImageToSaveAsync()
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            var saveFile = await savePicker.PickSaveFileAsync();

            return saveFile;
        }
    }
}