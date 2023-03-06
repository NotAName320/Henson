using Henson.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class PrepSelectedViewModel : ViewModelBase
    {
        private NsClient Client { get; }
        private List<NationLoginViewModel> Nations { get; set; }

        public PrepSelectedViewModel(List<NationLoginViewModel> nations, NsClient client)
        {
            Nations = nations;
            Client = client;
        }

        private string buttonText = "Login";
        public string ButtonText
        {
            get { return buttonText; }
            set
            {
                this.RaiseAndSetIfChanged(ref buttonText, value);
            }
        }
    }
}
