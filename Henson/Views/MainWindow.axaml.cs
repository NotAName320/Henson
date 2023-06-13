/*
Main Window control
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
            this.WhenActivated(d => d(ViewModel!.TagSelectedDialog.RegisterHandler(ShowTagSelectedDialog)));
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
            this.WhenActivated(d => d(ViewModel!.FileSaveDialog.RegisterHandler(ShowFilePickerDialog)));
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

        private async Task ShowPrepSelectedDialog(InteractionContext<PrepSelectedWindowViewModel, Unit> interaction)
        {
            var dialog = new PrepSelectedWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowTagSelectedDialog(InteractionContext<TagSelectedWindowViewModel, Unit> interaction)
        {
            var dialog = new TagSelectedWindow
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
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();

            var result = await messageBox.ShowDialog(this);
            interaction.SetOutput(result);
        }
        
        
        private async Task ShowFilePickerDialog(InteractionContext<ViewModelBase, string?> interaction)
        {
            //Need to write an error handler for this sometime
            var dialog = new SaveFileDialog
            {
                InitialFileName = "export.txt"
            };
            dialog.Filters!.Add(new FileDialogFilter { Name = "Dossier list", Extensions = { "txt" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "Swarm/Shine config", Extensions = { "json" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            interaction.SetOutput(result);
        }
    }
}
