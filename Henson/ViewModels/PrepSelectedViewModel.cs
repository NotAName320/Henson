using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class PrepSelectedViewModel : ViewModelBase
    {
        private List<NationLoginViewModel> Nations { get; set; }

        public PrepSelectedViewModel(List<NationLoginViewModel> nations)
        {
            Nations = nations;
        }
    }
}
