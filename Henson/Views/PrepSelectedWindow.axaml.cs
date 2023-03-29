/*
Prep Selected Window control
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
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowDialog(this);
            interaction.SetOutput(result);
        }
    }
}
