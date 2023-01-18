using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.Models
{
    public class Nation
    {
        public Nation(string name, string pass, string flagUrl)
        {
            Name = name;
            Pass = pass;
            FlagUrl = flagUrl;
        }

        public string Name { get; set; }
        public string Pass { get; set; }
        public string FlagUrl { get; set; }
        private string CachePath => $"./Cache/{Name}";
    }
}
