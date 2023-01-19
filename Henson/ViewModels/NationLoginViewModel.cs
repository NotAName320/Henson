using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class NationLoginViewModel : ViewModelBase
    {
        public NationLoginViewModel(string name, string pass)
        {
            Name = name;
            Pass = pass;
        }

        public string Name { get; set; }
        public string Pass { get; set; }
    }
}