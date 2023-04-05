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
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Henson.Models;
using log4net;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class TagSelectedWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The embassies, separated by commas, that the user would like to send in each region.
        /// </summary>
        public string Embassies
        {
            get => string.Join(',', _embassyList);
            set => _embassyList = value.Split(',').ToList();
        }
        private List<string> _embassyList = new();

        /// <summary>
        /// The WFE to tag each region with.
        /// </summary>
        public string WFE { get; set; } = "";

        /// <summary>
        /// The full file path to the banner image.
        /// </summary>
        public string BannerPath
        {
            get => bannerPath;
            set
            {
                bannerPath = value;
                if(bannerPath != "" && bannerPath.Contains('/'))
                {
                    BannerFileName = bannerPath.Split('/').Last();
                }
                else if(bannerPath != "" && bannerPath.Contains('\\'))
                {
                    BannerFileName = bannerPath.Split('\\').Last();
                }
            }
        }
        private string bannerPath = "";

        /// <summary>
        /// The full file path to the flag image.
        /// </summary>
        public string FlagPath
        {
            get => flagPath;
            set
            {
                flagPath = value;
                if(flagPath != "" && flagPath.Contains('/'))
                {
                    FlagFileName = flagPath.Split('/').Last();
                }
                else if(flagPath != "" && flagPath.Contains('\\'))
                {
                    FlagFileName = flagPath.Split('\\').Last();
                }
            }
        }
        private string flagPath = "";

        /// <summary>
        /// Fired when the Browse... button beside the banner text is clicked.
        /// </summary>
        public ICommand BannerPickerCommand { get; }

        /// <summary>
        /// Fired when the Browse... button beside the flag text is clicked.
        /// </summary>
        public ICommand FlagPickerCommand { get; }

        /// <summary>
        /// The command fired when pressing the action button.
        /// </summary>
        public ICommand ActionButtonCommand { get; }

        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// I should really create a common class for these lmao
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        /// <summary>
        /// This interaction opens the a file window, and returns a string array with the first value being the file chosen,
        /// or null if the window is closed without a pick.
        /// </summary>
        public Interaction<ViewModelBase, string[]?> FilePickerDialog { get; } = new();

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
        public string CurrentNation => _currentNation.Value;
        private readonly ObservableAsPropertyHelper<string> _currentNation;

        /// <summary>
        /// The current region being tagged.
        /// </summary>
        public string CurrentRegion => _currentRegion.Value;
        private readonly ObservableAsPropertyHelper<string> _currentRegion;

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

        public string BannerFileName
        {
            get => bannerFileName;
            set
            {
                this.RaiseAndSetIfChanged(ref bannerFileName, value);
            }
        }
        private string bannerFileName = "";

        public string FlagFileName
        {
            get => flagFileName;
            set
            {
                this.RaiseAndSetIfChanged(ref flagFileName, value);
            }
        }
        private string flagFileName = "";

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private NsClient Client { get; }

        /// <summary>
        /// The list of nations with RO perms to tag.
        /// </summary>
        private List<NationGridViewModel> NationsToTag { get; set; }

        /// <summary>
        /// The current index that the user is on.
        /// </summary>
        private int LoginIndex { get; set; } = 0;

        /// <summary>
        /// The current chk of the logged in nation.
        /// </summary>
        private string CurrentChk { get; set; } = "";

        /// <summary>
        /// The current Pin of the logged in nation.
        /// </summary>
        private string CurrentPin { get; set; } = "";

        /// <summary>
        /// The names of all the logins that failed.
        /// </summary>
        private StringBuilder FailedLogins { get; set; } = new();

        /// <summary>
        /// The log4net logger. It will emit messages as from TagSelectedWindowViewModel.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        public TagSelectedWindowViewModel(List<NationGridViewModel> nations, NsClient client)
        {
            NationsToTag = nations;
            Client = client;

            BannerPickerCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                var dialog = new ViewModelBase();
                var result = await FilePickerDialog.Handle(dialog);

                if(result != null) BannerPath = result[0];
            });

            FlagPickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new ViewModelBase();
                var result = await FilePickerDialog.Handle(dialog);

                if(result != null) FlagPath = result[0];
            });

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(WFE == "")
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "No WFE",
                        ContentMessage = $"Please set a WFE to tag regions with.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                if(BannerPath == "" || FlagPath == "")
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "No Banner/Flag",
                        ContentMessage = $"Please upload both a banner/flag to tag regions with.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationLoginViewModel currentNation = new(NationsToTag[LoginIndex].Name, NationsToTag[LoginIndex].Pass);

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(buttonText)
                {
                    case "Login":
                        var (chk, localId, pin) = Client.Login(currentNation) ?? default;
                        if(chk != null)
                        {
                            CurrentChk = chk;
                            CurrentPin = pin;

                            FooterText = $"Logged in to {currentNation.Name}.";
                            ButtonText = ""
                        }
                        else
                        {
                            FooterText = $"Login to {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                        }
                        break;
                    case "Change WFE":
                        break;
                }
            });

            this.WhenAnyValue(x => x.LoginIndex).Select(_ => LoginIndex == NationsToTag.Count ? "" : NationsToTag[LoginIndex].Name)
                .ToProperty(this, x => x.CurrentNation, out _currentNation);

            this.WhenAnyValue(x => x.LoginIndex).Select(_ => LoginIndex == NationsToTag.Count ? "" : NationsToTag[LoginIndex].Region)
                .ToProperty(this, x => x.CurrentRegion, out _currentRegion);
        }

        /// <summary>
        /// Adds a name to the failed logins and wraps the string around if necessary.
        /// </summary>
        /// <param name="loginName">The login name to be added.</param>
        private void AddToFailedLogins(string loginName)
        {
            log.Error($"Prepping {loginName} failed");
            if(FailedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                FailedLogins.Append('\n');
            }
            FailedLogins.Append(loginName + ", ");
        }
    }
}
