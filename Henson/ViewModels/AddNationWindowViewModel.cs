using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Henson.Models;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class AddNationWindowViewModel : ViewModelBase
    {
        public AddNationWindowViewModel()
        {
            ShowFilePickerDialog = new Interaction<FilePickerViewModel, string[]?>();

            FilePickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new FilePickerViewModel();

                var result = await ShowFilePickerDialog.Handle(dialog);

                if (result != null)
                {
                    JsonReader jsonReader = new(result[0]);
                    foreach(var keyValue in jsonReader.Items)
                    {
                        retVal.Add(new NationLoginViewModel(keyValue.Key, keyValue.Value));
                    }
                    return retVal;
                }

                return null;
            });
        }

        public string ImportOneUser { get; } = "";
        public string ImportOnePass { get; } = "";
        public string ImportManyUser { get; } = "";
        public string ImportManyPass { get; } = "";
        public string ImportManyRange { get; } = "";

        public ReactiveCommand<Unit, List<NationLoginViewModel>?> FilePickerCommand { get; }

        public Interaction<FilePickerViewModel, string[]?> ShowFilePickerDialog { get; }
 
        public List<NationLoginViewModel> retVal = new();

        public void ImportOne()
        {
            Console.WriteLine($"ImportOne {ImportOneUser} {ImportOnePass}");
        }

        public void ImportMany()
        {
            Console.WriteLine($"ImportMany {ImportManyUser} {ImportManyPass} {ImportManyRange}");
        }
    }
}
