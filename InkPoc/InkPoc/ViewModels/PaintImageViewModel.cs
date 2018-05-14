using System;
using System.Threading.Tasks;
using InkPoc.Helpers;
using InkPoc.Services;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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
            ?? (loadImageCommand = new RelayCommand(async () => ImageFile = await FileService.LoadImageAsync()));

        public RelayCommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync()));
        
        private async Task OnSaveImageAsync()
        {
            //var imageFile = await FileService.LoadImageAsync();
            //Image = await GetBitmapFromImageAsync(imageFile);
        }       
    }
}
