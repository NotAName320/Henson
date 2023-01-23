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
            this.WhenActivated(d => d(ViewModel!.SomeNationsFailedToPingDialog.RegisterHandler(ShowSomeNationsFailedToPingDialog)));
            this.WhenActivated(d => d(ViewModel!.NationPingSuccessDialog.RegisterHandler(ShowNationPingSuccessDialog)));
            this.WhenActivated(d => d(ViewModel!.FindWASuccessDialog.RegisterHandler(ShowFindWASuccessDialog)));
            this.WhenActivated(d => d(ViewModel!.WANotFoundDialog.RegisterHandler(ShowWANotFoundDialog)));
            this.WhenActivated(d => d(ViewModel!.LoginFailedDialog.RegisterHandler(ShowLoginFailedDialog)));
            this.WhenActivated(d => d(ViewModel!.NotCurrentLoginDialog.RegisterHandler(ShowNotCurrentLoginDialog)));
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

        private async Task ShowSomeNationsFailedToPingDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Warning",
                    ContentMessage = "One or more nation(s) failed to ping, probably due to an invalid username/password combo. They have been selected.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowNationPingSuccessDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Success",
                    ContentMessage = "All nations pinged successfully.",
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

        private async Task ShowLoginFailedDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Login Failed",
                    ContentMessage = "The login failed, probably due to an invalid username/password combination.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowNotCurrentLoginDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Current Login Doesn't Match",
                    ContentMessage = "Please log in with the the account you are trying to perform this action with.",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }
    }
}
