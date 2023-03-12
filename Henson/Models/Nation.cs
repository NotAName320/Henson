namespace Henson.Models
{
    public class Nation
    {
        public string Name { get; set; }
        public string Pass { get; set; }
        public string FlagUrl { get; set; }
        public string Region { get; set; }

        private string FlagCachePath => $"./Cache/{Name}";

        public Nation(string name, string pass, string flagUrl, string region)
        {
            Name = name;
            Pass = pass;
            FlagUrl = flagUrl;
            Region = region;
        }
    }
}
