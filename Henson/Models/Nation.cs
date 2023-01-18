using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.Models
{
    public class Nation
    {
        public Nation(string name, string pass, string flagUrl, string region, bool isChecked)
        {
            Name = name;
            Pass = pass;
            FlagUrl = flagUrl;
            Region = region;
            IsChecked = isChecked;
        }

        public string Name { get; set; }
        public string Pass { get; set; }
        public string FlagUrl { get; set; }
        public string Region { get; set; }
        public bool IsChecked { get; set; }
        private string FlagCachePath => $"./Cache/{Name}";

        public void Login()
        {
            System.Diagnostics.Debug.WriteLine("Login " + Name + " " + Pass);
        }

        public void ApplyWA()
        {
            System.Diagnostics.Debug.WriteLine("Apply WA " + Name + " " + Pass);
        }

        public void MoveTo(string region)
        {
            System.Diagnostics.Debug.WriteLine("MoveTo " + region);
        }
    }
}
