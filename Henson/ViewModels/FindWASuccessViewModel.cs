namespace Henson.ViewModels
{
    public class FindWASuccessViewModel : ViewModelBase
    {
        public FindWASuccessViewModel(string nation)
        {
            Nation = nation;
        }

        public string Nation { get; set; }
    }
}