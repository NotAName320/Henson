using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Henson.Models
{
    public class ConfigJsonReader
    {
        public Dictionary<string, string> Items { get; set; }

        public ConfigJsonReader(string filePath)
        {
            using StreamReader r = new(filePath);
            string jsonString = r.ReadToEnd();

            var allNations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonString);

            Items = allNations!.Values.First();
        }
    }
}
