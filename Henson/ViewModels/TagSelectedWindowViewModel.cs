/*
View Model representing Tag Selected Window
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


using System.Collections.Generic;
using Henson.Models;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class TagSelectedWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The embassies, separated by commas, that the user would like to send in each region.
        /// </summary>
        public string Embassies { get; set; } = "";

        /// <summary>
        /// The WFE to tag each region with.
        /// </summary>
        public string WFE { get; set; } = "";

        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// I should really create a common class for these lmao
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        /// <summary>
        /// The text on the button.
        /// </summary>
        public string ButtonText
        {
            get => buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonText, value);
            }
        }
        private string buttonText = "Login";

        /// <summary>
        /// The current logged in nation.
        /// </summary>
        public string CurrentNation
        {
            get => currentNation;
            set
            {
                this.RaiseAndSetIfChanged(ref currentNation, value);
            }
        }
        private string currentNation = "aowuehfpaowh";

        /// <summary>
        /// The current region being tagged.
        /// </summary>
        public string CurrentRegion
        {
            get => currentRegion;
            set
            {
                this.RaiseAndSetIfChanged(ref currentRegion, value);
            }
        }
        private string currentRegion = "ao[wiehfasdf]";

        /// <summary>
        /// The text displayed in the footer of the window.
        /// </summary>
        public string FooterText
        {
            get => footerText;
            set
            {
                this.RaiseAndSetIfChanged(ref footerText, value);
            }
        }
        private string footerText = "Click Login to start.";

        /// <summary>
        /// Boolean that controls the enabling and disabling of buttons that send requests
        /// to ensure compliance with API.
        /// </summary>
        public bool ButtonsEnabled
        {
            get => buttonsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonsEnabled, value);
            }
        }
        private bool buttonsEnabled = true;

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private NsClient Client { get; }

        /// <summary>
        /// The list of with RO perms to tag.
        /// </summary>
        private List<NationGridViewModel> NationsToTag { get; set; }

        public TagSelectedWindowViewModel(List<NationGridViewModel> nations, NsClient client, string target)
        {
            NationsToTag = nations;
            Client = client;

        }
    }
}
