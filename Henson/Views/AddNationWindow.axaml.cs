using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Henson.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Henson.Views
{
    public partial class AddNationWindow : ReactiveWindow<AddNationWindowViewModel>
    {
        public AddNationWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowFilePickerDialog.RegisterHandler(GetConfigJson)));
        }

        private async Task GetConfigJson(InteractionContext<FilePickerViewModel, string[]?> interaction)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });

            var result = await dialog.ShowAsync(this);
            if(result != null)
            {
                string fileName = result[0];
                string jsonString = File.ReadAllText(fileName);
                using var doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;
                var accounts = root.EnumerateArray();

                while(accounts.MoveNext())
                {
                    var account = accounts.Current;
                    System.Diagnostics.Debug.WriteLine(account);
                }
            }
            interaction.SetOutput(result);
        }
    }
}
