using Henson.Models;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Henson.ViewModels
{
    public class AddNationWindowViewModel : ViewModelBase
    {
        public string ImportOneUser { get; set; } = "";
        public string ImportOnePass { get; set; } = "";
        public string ImportManyUser { get; set; } = "";
        public string ImportManyPass { get; set; } = "";
        public string ImportManyRange { get; set; } = "";

        public ReactiveCommand<Unit, List<NationLoginViewModel>?> FilePickerCommand { get; }
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportOneCommand { get; }
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportManyCommand { get; }

        public Interaction<ViewModelBase, string[]?> FilePickerDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        public List<NationLoginViewModel> retVal = new();

        public AddNationWindowViewModel()
        {
            FilePickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new ViewModelBase();
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
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Error",
                        ContentMessage = "Please enter a username and/or password.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);

                    return null;
                }

                return new List<NationLoginViewModel>() { new NationLoginViewModel(ImportOneUser, ImportOnePass) };
            });

            ImportManyCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (string.IsNullOrWhiteSpace(ImportManyUser) || string.IsNullOrWhiteSpace(ImportManyPass) || string.IsNullOrWhiteSpace(ImportManyRange))
                {
                    MessageBoxViewModel errorDialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Error",
                        ContentMessage = "Please enter a username, password, and/or range (e.g. 1-50).",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(errorDialog);

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

                MessageBoxViewModel rangeDialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Error",
                    ContentMessage = "Please enter a valid range format (e.g. 1-50).",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(rangeDialog);

                return null;
            });
        }
    }
}
