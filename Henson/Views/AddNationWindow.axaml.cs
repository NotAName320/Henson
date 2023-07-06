/*
Add Nation Window control
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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Henson.Views
{
    public partial class AddNationWindow : ReactiveWindow<AddNationWindowViewModel>
    {
        public AddNationWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.FilePickerDialog.RegisterHandler(GetConfigJson)));
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
            this.WhenActivated(d => d(ViewModel!.FilePickerCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportOneCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportManyCommand.Subscribe(Close)));
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            //Spent like 4 hours figuring out that I needed the below line lol
            SetClosing(false);
        }

        private async Task GetConfigJson(InteractionContext<ViewModelBase, string[]?> interaction)
        {
            //Need to write an error handler for this sometime
            var dialog = new OpenFileDialog();
            dialog.Filters!.Add(new FileDialogFilter { Name = "Swarm/Shine config", Extensions = { "json", "toml" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            SetClosing(result == null); //If no file is selected, cancel window closing action
            interaction.SetOutput(result);
        }

        private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            parameters.WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
            parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(parameters);
            SetClosing(true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowDialog(this);
            interaction.SetOutput(result);
        }

        private void SetClosing(bool value)
        {
            Closing += (s, e) => { e.Cancel = value; };
        }
    }
}
