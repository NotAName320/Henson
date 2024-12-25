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
using Henson.ViewModels;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;

namespace Henson.Views
{
    public partial class AddNationWindow : HensonWindow<AddNationWindowViewModel>
    {
        public AddNationWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ConfigPickerDialog.RegisterHandler(GetConfigJson)));
            this.WhenActivated(d => d(ViewModel!.TextFilePickerDialog.RegisterHandler(GetTextFile)));
            this.WhenActivated(d => d(ViewModel!.ConfigPickerCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.TextPickerCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportOneCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportManyCommand.Subscribe(Close)));
        }

        private async Task GetConfigJson(IInteractionContext<ViewModelBase, string?> interaction)
        {
            var topLevel = GetTopLevel(this);
            var result = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Config File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new("Swarm/Shine config") { Patterns = new[] { "*.json", "*.toml" } },
                    FilePickerFileTypes.All
                }
            });
            
            SetClosing(result.Count == 0); //If no file is selected, cancel window closing action
            interaction.SetOutput(result.Count == 0 ? null : Uri.UnescapeDataString(result[0].Path.AbsolutePath));
        }

        private async Task GetTextFile(IInteractionContext<ViewModelBase, string?> interaction)
        {
            var topLevel = GetTopLevel(this);
            var result = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    FilePickerFileTypes.TextPlain,
                    FilePickerFileTypes.All
                }
            });
            
            SetClosing(result.Count == 0); //If no file is selected, cancel window closing action
            interaction.SetOutput(result.Count == 0 ? null : Uri.UnescapeDataString(result[0].Path.AbsolutePath));
        }

        protected override Task ShowMessageBoxDialog(IInteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            SetClosing(true);
            return base.ShowMessageBoxDialog(interaction);
        }
        
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            //Spent like 4 hours figuring out that I needed the below line lol
            SetClosing(false);
        }

        private void SetClosing(bool value)
        {
            Closing += (_, e) => { e.Cancel = value; };
        }
    }
}
