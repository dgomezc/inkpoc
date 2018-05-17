using System;

using InkPoc.Helpers;
using System;
using System.Threading.Tasks;
using InkPoc.Helpers;
using InkPoc.Services;
using Windows.UI.Input.Inking;

namespace InkPoc.ViewModels
{
    public class SmartCanvasViewModel : Observable
    {
        private string _recognizeText;
        private RelayCommand _recognizeTextCommand;
        private InkStrokeContainer _strokes;

        public SmartCanvasViewModel()
        {
            _strokes = new InkStrokeContainer();
        }

        public string RecognizeText
        {
            get => _recognizeText;
            set => Set(ref _recognizeText, value);
        }

        public InkStrokeContainer Strokes
        {
            get => _strokes;
            set => Set(ref _strokes, value);
        }

        public RelayCommand RecognizeTextCommand => _recognizeTextCommand
            ?? (_recognizeTextCommand = new RelayCommand(async () => await OnRecognizeText()));

        private async Task OnRecognizeText()
        {
            RecognizeText = await InkService.RecognizeTextAsync(Strokes);
        }
    }
}
