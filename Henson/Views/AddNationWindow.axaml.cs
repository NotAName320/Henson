using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reactive;
using System.Reactive.Linq;
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
            this.WhenActivated(d => d(ViewModel!.InvalidImportOneErrorDialog.RegisterHandler(ShowInvalidImportOneInputError)));
            this.WhenActivated(d => d(ViewModel!.InvalidImportManyErrorDialog.RegisterHandler(ShowInvalidImportManyInputError)));
            this.WhenActivated(d => d(ViewModel!.InvalidImportManyRangeErrorDialog.RegisterHandler(ShowInvalidImportManyRangeInputError)));
            this.WhenActivated(d => d(ViewModel!.FilePickerCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportOneCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportManyCommand.Subscribe(Close)));
        }
        
        private async Task GetConfigJson(InteractionContext<FilePickerViewModel, string[]?> interaction)
        {
            //Need to write an error handler for this sometime
            var dialog = new OpenFileDialog();
            dialog.Filters!.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            SetClosing(result == null); //If no file is selected, cancel window closing action
            interaction.SetOutput(result);
        }

        private async Task ShowInvalidImportOneInputError(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
              .GetMessageBoxStandardWindow(new MessageBoxStandardParams
              {
                  ContentTitle = "Error",
                  ContentMessage = "Please enter a username and/or password.",
                  Icon = MessageBox.Avalonia.Enums.Icon.Error,
                  WindowStartupLocation = WindowStartupLocation.CenterOwner,
              });
            SetClosing(true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowInvalidImportManyInputError(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Error",
                    ContentMessage = "Please enter a username, password, and/or range (e.g. 1-50).",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            SetClosing(true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private async Task ShowInvalidImportManyRangeInputError(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var messageBox = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ContentTitle = "Error",
                    ContentMessage = "Please enter a valid range format (e.g. 1-50).",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
            SetClosing(true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            interaction.SetOutput(await messageBox.ShowDialog(this));
        }

        private void SetClosing(bool value)
        {
            Closing += (s, e) => { e.Cancel = value; };
        }

        protected override void OnClosing(CancelEventArgs e)
        {    
            base.OnClosing(e);
            //Spent like 4 hours figuring out that I needed the below line lol
            SetClosing(false);
        }
    }
}
