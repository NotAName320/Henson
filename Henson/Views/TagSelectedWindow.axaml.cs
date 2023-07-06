/*
Tag Selected Window control
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
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.Views
{
    public partial class TagSelectedWindow : ReactiveWindow<TagSelectedWindowViewModel>
    {
        public TagSelectedWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
            this.WhenActivated(d => d(ViewModel!.FilePickerDialog.RegisterHandler(AddFilePicker)));
        }

        private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            parameters.WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
            parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(interaction.Input.Params);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowDialog(this);
            interaction.SetOutput(result);
        }

        private async Task AddFilePicker(InteractionContext<ViewModelBase, string[]?> interaction)
        {
            //Need to write an error handler for this sometime
            var dialog = new OpenFileDialog();
            dialog.Filters!.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            interaction.SetOutput(result);
        }
    }
}