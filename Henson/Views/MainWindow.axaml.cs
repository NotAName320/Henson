using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Media;
using System.Runtime.InteropServices;
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
            this.WhenActivated(d => d(ViewModel!.SomeNationsFailedToLoginDialog.RegisterHandler(ShowSomeNationsFailedToAddDialog)));
            this.WhenActivated(d => d(ViewModel!.NationLoginSuccessDialog.RegisterHandler(ShowNationLoginSuccessDialog)));
            this.WhenActivated(d => d(ViewModel!.FindWASuccessDialog.RegisterHandler(ShowFindWASuccessDialog)));
            this.WhenActivated(d => d(ViewModel!.WANotFoundDialog.RegisterHandler(ShowWANotFoundDialog)));
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowSomeNationsFailedToLoginDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Warning",
                    ContentMessage = "One or more nation(s) failed to login, probably due to an invalid username/password combo. They have been selected.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowNationLoginSuccessDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Success",
                    ContentMessage = "All nations logged in successfully.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowFindWASuccessDialog(InteractionContext<FindWASuccessViewModel, ButtonResult> interaction)
        {
            var WANation = interaction.Input.Nation;

            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "WA Nation Found",
                    ContentMessage = $"Your WA nation is {WANation}.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowWANotFoundDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "WA Nation Not Found",
                    ContentMessage = "Your WA nation was not found.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }
    }
}
