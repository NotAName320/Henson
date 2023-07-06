/*
Verify User Window control
Copyright (C) 2023 NotAName320

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.Views;

public partial class VerifyUserWindow : ReactiveWindow<VerifyUserWindowViewModel>
{
    public VerifyUserWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
        this.WhenActivated(d => d(ViewModel!.SubmitCommand.Subscribe(Close)));
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        SetClosing(false);
    }

    private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
    {
        var parameters = interaction.Input.Params;

        parameters.WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
        parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(interaction.Input.Params);
        SetClosing(true);
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

        var result = await messageBox.ShowDialog(this);
        interaction.SetOutput(result);
    }

    private void SetClosing(bool value)
    {
        Closing += (s, e) => { e.Cancel = value; };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}