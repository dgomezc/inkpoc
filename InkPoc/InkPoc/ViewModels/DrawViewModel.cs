
using System;
using System.Threading.Tasks;
using InkPoc.Helpers;
using InkPoc.Services.Ink;
using InkPoc.Services.Ink.UndoRedo;

namespace InkPoc.ViewModels
{
    public class DrawViewModel : Observable
    {
        private readonly InkLassoSelectionService lassoSelectionService;
        private readonly InkPointerDeviceService pointerDeviceService;
        private readonly InkCopyPasteService copyPasteService;
        private readonly InkUndoRedoService undoRedoService;
        private readonly InkFileService fileService;

        private RelayCommand cutCommand;
        private RelayCommand copyCommand;
        private RelayCommand pasteCommand;
        private RelayCommand undoCommand;
        private RelayCommand redoCommand;
        private RelayCommand loadInkFileCommand;
        private RelayCommand saveInkFileCommand;
        private RelayCommand exportInkFileCommand;

        private bool enableTouch;
        private bool enableMouse;
        private bool enableLassoSelection;

        public DrawViewModel()
        {
        }

        public DrawViewModel(
            InkLassoSelectionService _lassoSelectionService,
            InkPointerDeviceService _pointerDeviceService,
            InkCopyPasteService _copyPasteService,
            InkUndoRedoService _undoRedoService,
            InkFileService _fileService)
        {
            lassoSelectionService = _lassoSelectionService;
            pointerDeviceService = _pointerDeviceService;
            copyPasteService = _copyPasteService;
            undoRedoService = _undoRedoService;
            fileService = _fileService;

            EnableTouch = true;
            EnableMouse = true;
        }

        public RelayCommand CutCommand => cutCommand
           ?? (cutCommand = new RelayCommand(() =>
           {
               copyPasteService.Cut();
               lassoSelectionService.ClearSelection();
           }));

        public RelayCommand CopyCommand => copyCommand
           ?? (copyCommand = new RelayCommand(() =>
           {
               copyPasteService.Copy();
               lassoSelectionService.ClearSelection();
           }));

        public RelayCommand PasteCommand => pasteCommand
           ?? (pasteCommand = new RelayCommand(() =>
           {
               copyPasteService.Paste();
               lassoSelectionService.ClearSelection();
           }));

        public RelayCommand UndoCommand => undoCommand
           ?? (undoCommand = new RelayCommand(() =>
           {
               lassoSelectionService.ClearSelection();
               undoRedoService.Undo();
           }));

        public RelayCommand RedoCommand => redoCommand
           ?? (redoCommand = new RelayCommand(() =>
           {
               lassoSelectionService.ClearSelection();
               undoRedoService.Redo();
           }));
        
        public RelayCommand LoadInkFileCommand => loadInkFileCommand
           ?? (loadInkFileCommand = new RelayCommand(async () =>
           {
               lassoSelectionService.ClearSelection();
               var fileLoaded = await fileService.LoadInkAsync();

               if(fileLoaded)
               {
                   undoRedoService.Reset();
               }
           }));

        public RelayCommand SaveInkFileCommand => saveInkFileCommand
           ?? (saveInkFileCommand = new RelayCommand(async () =>
           {
               lassoSelectionService.ClearSelection();
               await fileService.SaveInkAsync();
           }));

        public RelayCommand ExportInkFileCommand => exportInkFileCommand
           ?? (exportInkFileCommand = new RelayCommand(async () =>
           {
               lassoSelectionService.ClearSelection();
               await fileService.ExportToImageAsync();
           }));
        
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
        
        public bool EnableLassoSelection
        {
            get => enableLassoSelection;
            set
            {
                Set(ref enableLassoSelection, value);
                ConfigLassoSelection(value);
            }
        }

        private void ConfigLassoSelection(bool enableLasso)
        {
            if(enableLasso)
            {
                lassoSelectionService.StartLassoSelectionConfig();
            }
            else
            {
                lassoSelectionService.EndLassoSelectionConfig();
            }
        }
    }
}
