namespace Henson.ViewModels
{
    public class NationLoginViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string Pass { get; set; }

        public NationLoginViewModel(string name, string pass)
        {
            Name = name;
            Pass = pass;
        }
    }
}