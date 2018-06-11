using InkPoc.Helpers;
using InkPoc.Services.Ink;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media;

namespace InkPoc.ViewModels
{
    public class PaintImageViewModel : Observable
    {
        private bool enableTouch;
        private bool enableMouse;
        private ImageSource image;
        private StorageFile imageFile;

        private readonly InkStrokesService strokesService;
        private readonly InkPointerDeviceService pointerDeviceService;
        private readonly InkFileService fileService;

        private RelayCommand loadImageCommand;
        private RelayCommand saveImageCommand;
        private RelayCommand clearAllCommand;

        public PaintImageViewModel()
        {
        }
        
        public PaintImageViewModel(
            InkStrokesService _strokesService,
            InkPointerDeviceService _pointerDeviceService,
            InkFileService _fileService)
        {
            strokesService = _strokesService;
            pointerDeviceService = _pointerDeviceService;
            fileService = _fileService;

            enableMouse = true;
            EnableTouch = true;
        }

        public bool EnableTouch
        {
            get => enableTouch;
            set
            {
                Set(ref enableTouch, value);
                pointerDeviceService.EnableTouch = value;
            }
        }

        public bool EnableMouse
        {
            get => enableMouse;
            set
            {
                Set(ref enableMouse, value);
                pointerDeviceService.EnableMouse = value;
            }
        }

        public ImageSource Image
        {
            get => image;
            set => Set(ref image, value);
        }

        public RelayCommand LoadImageCommand => loadImageCommand
            ?? (loadImageCommand = new RelayCommand(async () => await OnLoadImageAsync()));

        public RelayCommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync()));
        
        private async Task OnLoadImageAsync()
        {
            var openPicker = new FileOpenPicker();

            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");

            imageFile = await openPicker.PickSingleFileAsync();
            Image = await ImageHelper.GetBitmapFromImageAsync(imageFile);
        }

        private async Task OnSaveImageAsync()
        {
            await fileService.ExportToImageAsync(imageFile);
        }

        public RelayCommand ClearAllCommand => clearAllCommand
           ?? (clearAllCommand = new RelayCommand(ClearAll));

        private void ClearAll()
        {
            strokesService.ClearStrokes();
            imageFile = null;
            Image = null;
        }
    }
}
