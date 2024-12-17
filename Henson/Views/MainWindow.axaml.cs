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


using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Media;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;

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
            this.WhenActivated(d => d(ViewModel!.VerifyUserDialog.RegisterHandler(ShowVerifyUserDialog)));
            this.WhenActivated(d => d(ViewModel!.FilterNationsDialog.RegisterHandler(ShowFilterNationsDialog)));
            this.WhenActivated(d =>
            {
                d(ViewModel!.PerformChecks.RegisterHandler(ctx => ctx.SetOutput(Unit.Default)));
                // why are we still here? just to suffer?
                RxApp.MainThreadScheduler.Schedule(ViewModel!.DoStartupChecks);
            });
        }

        private async Task ShowAddNationDialog(IInteractionContext<AddNationWindowViewModel, List<NationLoginViewModel>?> interaction)
        {
            var dialog = new AddNationWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<List<NationLoginViewModel>?>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowPrepSelectedDialog(IInteractionContext<PrepSelectedWindowViewModel, Unit> interaction)
        {
            var dialog = new PrepSelectedWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowTagSelectedDialog(IInteractionContext<TagSelectedWindowViewModel, Unit> interaction)
        {
            var dialog = new TagSelectedWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowMessageBoxDialog(IInteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            parameters.WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));
            parameters.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(interaction.Input.Params);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            
            var result = await messageBox.ShowWindowDialogAsync(this);
            interaction.SetOutput(result);
        }
        
        
        private async Task ShowFilePickerDialog(IInteractionContext<ViewModelBase, string?> interaction)
        {
            var topLevel = GetTopLevel(this);
            var result = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Nations",
                SuggestedFileName = "export.txt",
                FileTypeChoices = new[]
                {
                    FilePickerFileTypes.TextPlain,
                    new("Swarm/Shine config") { Patterns = new[] { "*.json" } },
                    FilePickerFileTypes.All
                }
            });
            interaction.SetOutput(result == null ? null : Uri.UnescapeDataString(result.Path.AbsolutePath));
        }

        private async Task ShowVerifyUserDialog(IInteractionContext<VerifyUserWindowViewModel, string?> interaction)
        {
            var dialog = new VerifyUserWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<string?>(this);
            interaction.SetOutput(result);

        }

        private async Task ShowFilterNationsDialog(
            IInteractionContext<FilterNationsWindowViewModel, (int, string, bool?, bool)?>
                interaction)
        {
            var dialog = new FilterNationsWindow
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<(int, string, bool?, bool)?>(this);
            interaction.SetOutput(result);
        }
        
        private void NationList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            NationList.SelectedItem = null;
        }

        private void WindowBase_OnOpened(object? sender, EventArgs e)
        {
            //cant change theme without window opening
            ViewModel!.SetSettings();
        }
    }
}
