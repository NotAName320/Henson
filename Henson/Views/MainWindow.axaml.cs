using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Media;
using System.Threading.Tasks;

namespace Henson.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.AddNationDialog.RegisterHandler(ShowAddNationDialog)));
            this.WhenActivated(d => d(ViewModel!.RemoveNationConfirmationDialog.RegisterHandler(ShowRemoveNationConfirmationDialog)));
            this.WhenActivated(d => d(ViewModel!.SomeNationsFailedToAddDialog.RegisterHandler(ShowSomeNationsFailedToAddDialog)));
        }

        private async Task ShowAddNationDialog(InteractionContext<AddNationWindowViewModel, List<NationLoginViewModel>?> interaction)
        {
            var dialog = new AddNationWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<List<NationLoginViewModel>?>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowRemoveNationConfirmationDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Remove Selected Nations",
                    ContentMessage = "Are you sure you want to remove the selected nations' logins from Henson?",
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    ButtonDefinitions = ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));

        }

        private async Task ShowSomeNationsFailedToAddDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Warning",
                    ContentMessage = "One or more nation(s) failed to add, probably due to an invalid username/password combo.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }
    }
}
