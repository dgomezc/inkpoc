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
        private ImageSource image;
        private RelayCommand loadImageCommand;
        private RelayCommand saveImageCommand;
        private InkStrokeContainer strokes;

        public PaintImageViewModel()
        {
            strokes = new InkStrokeContainer();
        }

        public ImageSource Image
        {
            get => image;
            set
            {
                Set(ref image, value);
                OnPropertyChanged(nameof(HasImage));
            }
        }

        public bool HasImage => Image != null;

        public InkStrokeContainer Strokes
        {
            get => strokes;
            set => Set(ref strokes, value);
        }

        public RelayCommand LoadImageCommand => loadImageCommand
            ?? (loadImageCommand = new RelayCommand(async () => await OnLoadImageAsync()));

        public RelayCommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync()));
        
        private async Task OnLoadImageAsync()
        {
            var imageFile = await FileService.LoadImageAsync();
            Image = await GetBitmapFromImageAsync(imageFile);
        }

        private async Task OnSaveImageAsync()
        {
            //var imageFile = await FileService.LoadImageAsync();
            //Image = await GetBitmapFromImageAsync(imageFile);
        }

        private async Task<BitmapImage> GetBitmapFromImageAsync(StorageFile file)
        {
            if (file == null)
            {
                return null;
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                return bitmapImage;
            }
        }
    }
}
