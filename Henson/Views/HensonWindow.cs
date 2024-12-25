using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.MessageBox;
using Henson.ViewModels;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.Views;

public class HensonWindow<T> : ReactiveWindow<T> where T : ViewModelBase
{
    protected HensonWindow()
    {
        this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
    }

    protected virtual async Task ShowMessageBoxDialog(IInteractionContext<MessageBoxViewModel, ButtonResult> interaction)
    {
        var vm = interaction.Input;
        var vmParams = vm.Params;

        vmParams.WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
        vmParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var messageBox = MessageBoxManager.GetMessageBoxStandard(vmParams, vm.WindowStyling);
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

        var result = await messageBox.ShowWindowDialogAsync(this);
        interaction.SetOutput(result);
    }
}