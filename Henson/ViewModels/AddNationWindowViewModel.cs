using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Henson.Models;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class AddNationWindowViewModel : ViewModelBase
    {
        public AddNationWindowViewModel()
        {
            FilePickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new FilePickerViewModel();
                var result = await FilePickerDialog.Handle(dialog);

                if (result != null)
                {
                    ConfigJsonReader jsonReader = new(result[0]);
                    foreach(var keyValue in jsonReader.Items)
                    {
                        retVal.Add(new NationLoginViewModel(keyValue.Key, keyValue.Value));
                    }
                    return retVal;
                }

                return null;
            });

            ImportOneCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(string.IsNullOrWhiteSpace(ImportOneUser) || string.IsNullOrWhiteSpace(ImportOnePass))
                {
                    var dialog = new MessageBoxViewModel();
                    await InvalidImportOneErrorDialog.Handle(dialog);

                    return null;
                }

                return new List<NationLoginViewModel>() { new NationLoginViewModel(ImportOneUser, ImportOnePass) };
            });

            ImportManyCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new MessageBoxViewModel();
                if (string.IsNullOrWhiteSpace(ImportManyUser) || string.IsNullOrWhiteSpace(ImportManyPass) || string.IsNullOrEmpty(ImportManyRange))
                {
                    await InvalidImportManyErrorDialog.Handle(dialog);

                    return null;
                }

                string[] range = ImportManyRange.Split("-");
                List<NationLoginViewModel> retVal = new();

                if(range.Length >= 2 && Int32.TryParse(range[0], out int start) && Int32.TryParse(range[1], out int end))
                {
                    for(int i = start; i <= end; i++)
                    {
                        var user = ImportManyUser.Contains('*') ? ImportManyUser.Replace("*", i.ToString()) : $"{ImportManyUser} {i}";
                        retVal.Add(new NationLoginViewModel(user, ImportManyPass));
                    }
                    return retVal;
                }

                await InvalidImportManyRangeErrorDialog.Handle(dialog);

                return null;
            });
        }

        public string ImportOneUser { get; set; } = "";
        public string ImportOnePass { get; set;  } = "";
        public string ImportManyUser { get; set; } = "";
        public string ImportManyPass { get; set; } = "";
        public string ImportManyRange { get; set; } = "";

        public ReactiveCommand<Unit, List<NationLoginViewModel>?> FilePickerCommand { get; }
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportOneCommand { get; }
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportManyCommand { get; }

        public Interaction<FilePickerViewModel, string[]?> FilePickerDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> InvalidImportOneErrorDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> InvalidImportManyErrorDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> InvalidImportManyRangeErrorDialog { get; } = new();

        public List<NationLoginViewModel> retVal = new();
    }
}
