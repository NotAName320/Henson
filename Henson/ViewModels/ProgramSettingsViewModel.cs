using ReactiveUI;

namespace Henson.ViewModels
{
    public class ProgramSettingsViewModel : ViewModelBase
    {
        public string UserAgent
        {
            get => userAgent;
            set
            {
                this.RaiseAndSetIfChanged(ref userAgent, value);
            }
        }
        private string userAgent = "";

        public int Theme
        {
            get => theme;
            set
            {
                this.RaiseAndSetIfChanged(ref theme, value);
            }
        }
        private int theme = 0;
    }
}
