using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace InkPoc.Controls
{
    public class FileImportExportInkOption : InkOption
    {
        private const string OpenFileLabel = "Open file";
        private const string SaveFileLabel = "Save file";
        private const string ExportAsImageLabel = "Export as image";

        private AppBarButton _openFileButton;
        private AppBarButton _saveFileButton;
        private AppBarButton _exportAsImageButton;

        public AppBarButton OpenFileButton => _openFileButton ?? (_openFileButton = BuildAppBarButton(OpenFileLabel, Symbol.OpenFile));

        public AppBarButton SaveFileButton => _saveFileButton ?? (_saveFileButton = BuildAppBarButton(SaveFileLabel, Symbol.Save));

        public AppBarButton ExportAsImageButton => _exportAsImageButton ?? (_exportAsImageButton = BuildAppBarButton(ExportAsImageLabel, Symbol.Download));
    }
}
