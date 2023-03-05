using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Media;
using System.Reactive;
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
            this.WhenActivated(d => d(ViewModel!.PrepSelectedDialog.RegisterHandler(ShowPrepSelectedDialog)));
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
        }

        private async Task ShowAddNationDialog(InteractionContext<AddNationWindowViewModel, List<NationLoginViewModel>?> interaction)
        {
            var dialog = new AddNationWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<List<NationLoginViewModel>?>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowPrepSelectedDialog(InteractionContext<PrepSelectedViewModel, Unit> interaction)
        {
            var dialog = new PrepSelectedWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            parameters.WindowIcon = new WindowIcon(new Bitmap(assets!.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
            parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(interaction.Input.Params);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }
    }
}
