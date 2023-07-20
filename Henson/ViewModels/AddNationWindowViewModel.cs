/*
View Model representing Add Nation Window
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


using Henson.Models;
using MessageBox.Avalonia.DTO;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using log4net;
using MsBox.Avalonia.Enums;

namespace Henson.ViewModels
{
    public class AddNationWindowViewModel : ViewModelBase
    {
        //strings that are bound to the textboxes
        public string ImportTextPass { get; set; } = "";
        public string ImportOneUser { get; set; } = "";
        public string ImportOnePass { get; set; } = "";
        public string ImportManyUser { get; set; } = "";
        public string ImportManyPass { get; set; } = "";
        public string ImportManyRange { get; set; } = "";

        /// <summary>
        /// Fired when the Browse... button to select a Swarm/Shine config is clicked.
        /// </summary>
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ConfigPickerCommand { get; }
        
        /// <summary>
        /// Fired when the Browse... button to select a text file is clicked.
        /// </summary>
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> TextPickerCommand { get; }

        /// <summary>
        /// Fired when the Import button is clicked.
        /// </summary>
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportOneCommand { get; }

        /// <summary>
        /// Fired when the Mass Import button is clicked.
        /// </summary>
        public ReactiveCommand<Unit, List<NationLoginViewModel>?> ImportManyCommand { get; }

        /// <summary>
        /// This interaction opens the config file window, and returns a string array with the first value being the
        /// file chosen, or null if the window is closed without a pick.
        /// </summary>
        public Interaction<ViewModelBase, string?> ConfigPickerDialog { get; } = new();
        
        /// <summary>
        /// This interaction opens the text file window, and returns a string array with the first value being the file
        /// chosen, or null if the window is closed without a pick.
        /// </summary>
        public Interaction<ViewModelBase, string?> TextFilePickerDialog { get; } = new();


        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();
        
        /// <summary>
        /// The log4net logger. It will emit messages as from AddNationWindowViewModel.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Constructs a new <c>AddWindowViewModel</c>.
        /// </summary>
        public AddNationWindowViewModel()
        {
            ConfigPickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new ViewModelBase();
                var result = await ConfigPickerDialog.Handle(dialog);

                if(result == null) return null;
                ConfigReader jsonReader;
                try
                {
                    jsonReader = new ConfigReader(result);
                }
                catch (Exception e)
                {
                    MessageBoxViewModel messageDialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Config Processing Error",
                        ContentMessage = "The config file was invalid.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(messageDialog);
                    
                    return null;
                }

                return jsonReader.Items.Select(keyValue => new NationLoginViewModel(keyValue.Key, keyValue.Value))
                    .ToList();

            });

            TextPickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(ImportTextPass == "")
                {
                    MessageBoxViewModel messageDialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "No Password Set",
                        ContentMessage = "Please input a password before selecting a file.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(messageDialog);

                    return null;
                }
                
                var dialog = new ViewModelBase();
                var result = await TextFilePickerDialog.Handle(dialog);

                if(result == null) return null;
                var lines = File.ReadLines(result);

                return lines.Select(line => new NationLoginViewModel(line, ImportTextPass)).ToList();

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

                return new List<NationLoginViewModel> { new(ImportOneUser, ImportOnePass) };
            });

            ImportManyCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(string.IsNullOrWhiteSpace(ImportManyUser) || string.IsNullOrWhiteSpace(ImportManyPass) || string.IsNullOrWhiteSpace(ImportManyRange))
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
                        string user;
                        if(ImportManyUser.Contains('*') || ImportManyUser.Contains('^') || ImportManyUser.Contains('%'))
                        {
                            user = ImportManyUser.Replace("*", i.ToString()).Replace("^", ToRoman(i))
                                .Replace("%", ToOrdinal(i));
                        }
                        else
                        {
                            user = $"{ImportManyUser} {i}";
                        }
                        Debug.WriteLine(user);
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
        
        private static string ToRoman(int number)
        {
            return number switch
            {
                < 1 => string.Empty,
                >= 1000 => "M" + ToRoman(number - 1000),
                >= 900 => "CM" + ToRoman(number - 900),
                >= 500 => "D" + ToRoman(number - 500),
                >= 400 => "CD" + ToRoman(number - 400),
                >= 100 => "C" + ToRoman(number - 100),
                >= 90 => "XC" + ToRoman(number - 90),
                >= 50 => "L" + ToRoman(number - 50),
                >= 40 => "XL" + ToRoman(number - 40),
                >= 10 => "X" + ToRoman(number - 10),
                >= 9 => "IX" + ToRoman(number - 9),
                >= 5 => "V" + ToRoman(number - 5),
                >= 4 => "IV" + ToRoman(number - 4),
                >= 1 => "I" + ToRoman(number - 1)
            };
        }

        private static string ToOrdinal(int number)
        {
            if( number <= 0 ) return number.ToString();

            switch(number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            return (number % 10) switch
            {
                1 => number + "st",
                2 => number + "nd",
                3 => number + "rd",
                _ => number + "th"
            };
        }
    }
}
