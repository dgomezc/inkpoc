using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace InkPoc.Helpers
{
    public static class ImageHelper
    {
        public static async Task<BitmapImage> GetBitmapFromImageAsync(StorageFile file)
        {
            if(file == null)
            {
                return null;
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
            }
        }
    }
}
