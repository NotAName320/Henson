using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
