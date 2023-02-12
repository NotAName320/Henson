using ReactiveUI;

namespace Henson.ViewModels
{
    public class ProgramSettingsViewModel : ViewModelBase
    {
        private string userAgent = "";
        public string UserAgent
        {
            get => userAgent;
            set
            {
                this.RaiseAndSetIfChanged(ref userAgent, value);
            }
        }
    }
}
