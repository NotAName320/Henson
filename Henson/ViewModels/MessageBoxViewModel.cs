using MessageBox.Avalonia.DTO;

namespace Henson.ViewModels
{
    public class MessageBoxViewModel : ViewModelBase
    {
        public MessageBoxStandardParams Params { get; set; }

        public MessageBoxViewModel(MessageBoxStandardParams messageParams)
        {
            Params = messageParams;
        }
    }
}
