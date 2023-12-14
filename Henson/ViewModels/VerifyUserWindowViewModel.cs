/*
View Model representing Verify User Window.
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


using System.Reactive;
using System.Reactive.Linq;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class VerifyUserWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The checksum inputted into the text box.
        /// </summary>
        public string Checksum { get; set; } = "";
        
        /// <summary>
        /// Fired when the Submit button is clicked.
        /// </summary>
        public ReactiveCommand<Unit, string?> SubmitCommand { get; }
        
        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();


        public VerifyUserWindowViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(Checksum != "") return Checksum;
                var messageDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "No Checksum Provided",
                    ContentMessage = "Please go to the link provided and get your code.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(messageDialog);
                    
                return null;
            });
        }
    }
}
