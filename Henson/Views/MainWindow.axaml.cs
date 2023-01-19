using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;

namespace Henson.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowAddNationDialog.RegisterHandler(DoShowDialogAsync)));
        }

        private async Task DoShowDialogAsync(InteractionContext<AddNationWindowViewModel, AddNationOptionViewModel?> interaction)
        {
            var dialog = new AddNationWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<AddNationOptionViewModel?>(this);
            interaction.SetOutput(result);
        }
    }
}
