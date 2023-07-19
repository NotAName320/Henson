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
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MsBox.Avalonia.Enums;
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

            var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(interaction.Input.Params);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowWindowDialogAsync(this);
            interaction.SetOutput(result);
        }

        private async Task AddFilePicker(InteractionContext<ViewModelBase, string?> interaction)
        {
            var topLevel = GetTopLevel(this);
            var result = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Image File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    FilePickerFileTypes.ImageAll,
                    FilePickerFileTypes.All
                }
            });
            
            interaction.SetOutput(result.Count == 0 ? null : result[0].Path.AbsolutePath);
        }

        private void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
        {
            if((e.Key != Key.Space && e.Key != Key.Enter) || !ViewModel!.ButtonsEnabled) return;
            ViewModel!.ActionButtonCommand.Execute(null);
        }
    }
}