/*
Object that reads Swarm config.json
Copyright (C) 2023 NotAName320

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


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
