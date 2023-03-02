using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Media;
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
            this.WhenActivated(d => d(ViewModel!.MessageBoxDialog.RegisterHandler(ShowMessageBoxDialog)));
            this.WhenActivated(d => d(ViewModel!.FilePickerCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportOneCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.ImportManyCommand.Subscribe(Close)));
        }
        
        private async Task GetConfigJson(InteractionContext<ViewModelBase, string[]?> interaction)
        {
            //Need to write an error handler for this sometime
            var dialog = new OpenFileDialog();
            dialog.Filters!.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            SetClosing(result == null); //If no file is selected, cancel window closing action
            interaction.SetOutput(result);
        }

        private async Task ShowMessageBoxDialog(InteractionContext<MessageBoxViewModel, ButtonResult> interaction)
        {
            var parameters = interaction.Input.Params;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            parameters.WindowIcon = new WindowIcon(new Bitmap(assets!.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))));

            var messageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(parameters);
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
