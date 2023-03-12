using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using Avalonia.Media.Imaging;

namespace Henson.Views
{
    public partial class PrepSelectedWindow : ReactiveWindow<PrepSelectedViewModel>
    {
        public PrepSelectedWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
        }

        private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            parameters.WindowIcon = new WindowIcon(new Bitmap(assets!.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
            parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(interaction.Input.Params);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowDialog(this);
            interaction.SetOutput(result);
        }
    }
}
