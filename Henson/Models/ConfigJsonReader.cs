using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Henson.Models
{
    public class ConfigJsonReader
    {
        public ConfigJsonReader(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string jsonString = r.ReadToEnd();

                var allNations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonString);

                Items = allNations.Values.First();
            }
        }

        public Dictionary<string, string> Items { get; set; }
    }
}
