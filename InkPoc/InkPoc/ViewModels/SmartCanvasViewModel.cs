
using InkPoc.Helpers;
using InkPoc.Services;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace InkPoc.ViewModels
{
    public class SmartCanvasViewModel : Observable
    {
        private string _recognizeText;
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
        
    }
}
