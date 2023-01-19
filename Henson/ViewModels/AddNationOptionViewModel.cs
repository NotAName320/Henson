using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class AddNationOptionViewModel : ViewModelBase
    {
        public AddNationOptionViewModel(Option option, List<NationLoginViewModel> input)
        {
        }

        //public AddNationOptionViewModel(Option option, )

        public enum Option
        {
            ImportOne,
            ImportMany,
            ImportFile
        }
    }
}
