using InkPoc.Helpers;
using InkPoc.Services.Ink;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace InkPoc.ViewModels
{
    public class PaintImageViewModel : Observable
    {
        private bool enableTouch;
        private bool enableMouse;
        private BitmapImage image;
        private StorageFile imageFile;

        private readonly InkStrokesService strokesService;
        private readonly InkPointerDeviceService pointerDeviceService;
        private readonly InkFileService fileService;
        private readonly InkZoomService zoomService;

        private RelayCommand loadImageCommand;
        private RelayCommand saveImageCommand;
        private RelayCommand clearAllCommand;
        private RelayCommand zoomInCommand;
        private RelayCommand zoomOutCommand;

        public PaintImageViewModel()
        {
        }
        
        public PaintImageViewModel(
            InkStrokesService _strokesService,
            InkPointerDeviceService _pointerDeviceService,
            InkFileService _fileService,
            InkZoomService _zoomService)
        {
            strokesService = _strokesService;
            pointerDeviceService = _pointerDeviceService;
            fileService = _fileService;
            zoomService = _zoomService;

            enableMouse = true;
            EnableTouch = true;

            pointerDeviceService.DetectPenEvent += (s, e) => EnableTouch = false;
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

        public BitmapImage Image
        {
            get => image;
            set => Set(ref image, value);
        }

        public RelayCommand LoadImageCommand => loadImageCommand
            ?? (loadImageCommand = new RelayCommand(async () => await OnLoadImageAsync()));

        public RelayCommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync()));

        public RelayCommand ZoomInCommand => zoomInCommand
            ?? (zoomInCommand = new RelayCommand(() => zoomService.ZoomIn()));

        public RelayCommand ZoomOutCommand => zoomOutCommand
            ?? (zoomOutCommand = new RelayCommand(() => zoomService.ZoomOut()));

        private async Task OnLoadImageAsync()
        {
            imageFile = await ImageHelper.LoadImageFileAsync();
            Image = await ImageHelper.GetBitmapFromImageAsync(imageFile);

            if(Image != null)
            {
                var imageSize = new Size(Image.PixelWidth, Image.PixelHeight);
                zoomService.AdjustToSize(imageSize);
            }           
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
