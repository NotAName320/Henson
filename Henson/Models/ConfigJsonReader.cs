using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Henson.Models
{
    public class ConfigJsonReader
    {
        /// <summary>
        /// A dictionary containg key-value pairs from the config.json.
        /// </summary>
        public Dictionary<string, string> Items { get; set; }

        /// <summary>
        /// Constructs a new <c>ConfigJsonReader</c> from a file.
        /// </summary>
        /// <param name="filePath">The string path to the file.</param>
        public ConfigJsonReader(string filePath)
        {
            using StreamReader r = new(filePath);
            string jsonString = r.ReadToEnd();

            var allNations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonString);

            Items = allNations!.Values.First();
        }
    }
}
