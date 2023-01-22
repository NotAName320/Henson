using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class FindWASuccessViewModel : ViewModelBase
    {
        public FindWASuccessViewModel(string nation)
        {
            Nation = nation;
        }

        public string Nation { get; set; }
    }
}