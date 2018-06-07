using InkPoc.Helpers;
using InkPoc.Services;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Input.Inking;

namespace InkPoc.ViewModels
{
    public class PaintImageViewModel : Observable
    {
        private StorageFile imageFile;
        private RelayCommand loadImageCommand;
        private RelayCommand saveImageCommand;
        private InkStrokeContainer strokes;

        public PaintImageViewModel()
        {
            strokes = new InkStrokeContainer();
        }

        public StorageFile ImageFile
        {
            get => imageFile;
            set
            {
                Set(ref imageFile, value);
                OnPropertyChanged(nameof(HasImage));
            }
        }

        public bool HasImage => ImageFile != null;

        public InkStrokeContainer Strokes
        {
            get => strokes;
            set => Set(ref strokes, value);
        }

        public RelayCommand LoadImageCommand => loadImageCommand
            ?? (loadImageCommand = new RelayCommand(async () => await OnLoadImageAsync()));

        public RelayCommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync()));
        
        private async Task OnSaveImageAsync()
        {
            await Task.CompletedTask;
            //await InkService.ExportToImageAsync(strokes, CanvasSize, ImageFile);
        }

        public Size CanvasSize { get; set; }

        private async Task OnLoadImageAsync()
        {
            var openPicker = new FileOpenPicker();

            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            if(file != null)
            {
                ImageFile = file;
            }
        }
    }
}
