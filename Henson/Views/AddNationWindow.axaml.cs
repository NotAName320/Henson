using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace Henson.Views
{
    public partial class AddNationWindow : ReactiveWindow<AddNationWindowViewModel>
    {
        public AddNationWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowFilePickerDialog.RegisterHandler(GetConfigJson)));
            this.WhenActivated(d => d(ViewModel!.FilePickerCommand.Subscribe(Close)));
        }
        
        private async Task GetConfigJson(InteractionContext<FilePickerViewModel, string[]?> interaction)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            
            if(result == null) //If no file is selected, cancel window closing action
            {
                this.Closing += (s, e) =>
                {
                    e.Cancel = true;
                };
            }
            else
            {
                this.Closing += (s, e) =>
                {
                    e.Cancel = false;
                };
            }

            interaction.SetOutput(result);
        }
    }
}
