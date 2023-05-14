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
            set => _embassyList = value.Split(',').Select(x => x.Trim()).ToList();
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

        public bool EmbassiesEnabled
        {
            get => embassiesEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref embassiesEnabled, value);
            }
        }
        private bool embassiesEnabled = false;

        public bool FlagBannerEnabled
        {
            get => flagBannerEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref flagBannerEnabled, value);
            }
        }
        private bool flagBannerEnabled = false;

        public bool WFEEnabled
        {
            get => wFEEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref wFEEnabled, value);
            }
        }
        private bool wFEEnabled = false;

        public bool TagsEnabled
        {
            get => tagsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref tagsEnabled, value);
            }
        }
        private bool tagsEnabled = false;

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
        /// The current index that the user is on.
        /// </summary>
        public int LoginIndex
        {
            get => loginIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref loginIndex, value);
            }
        }
        private int loginIndex = 0;

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private readonly NsClient Client;

        /// <summary>
        /// The list of nations with RO perms to tag.
        /// </summary>
        private readonly List<NationGridViewModel> NationsToTag;

        /// <summary>
        /// The embassy index that the user is on.
        /// </summary>
        private int EmbIndex = 0;

        /// <summary>
        /// The current chk of the logged in nation.
        /// </summary>
        private string CurrentChk = "";

        /// <summary>
        /// The current Pin of the logged in nation.
        /// </summary>
        private string CurrentPin = "";

        /// <summary>
        /// The current Banner ID uploaded to the region.
        /// </summary>
        private string CurrentBannerID = "";

        /// <summary>
        /// The current Flag ID uploaded to the region.
        /// </summary>
        private string CurrentFlagID = "";

        /// <summary>
        /// The names of all the logins that failed.
        /// </summary>
        private readonly StringBuilder FailedLogins = new();

        /// <summary>
        /// A list of embassies to close.
        /// </summary>
        private List<string> EmbassiesToClose = new();

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
                //if none of the tag features are enabled
                if((new bool[] { EmbassiesEnabled, FlagBannerEnabled, WFEEnabled, TagsEnabled }).All(x => !x))
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Nothing Selected",
                        ContentMessage = $"Please select something to do to the regions.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;

                }

                if(WFEEnabled && WFE == "")
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

                if(FlagBannerEnabled && (BannerPath == "" || FlagPath == ""))
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

                if(LoginIndex == NationsToTag.Count)
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Logins Complete",
                        ContentMessage = $"All regions have been tagged. Please close the window now.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationGridViewModel currentNation = NationsToTag[LoginIndex];

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(ButtonText) //very dumb mindless code who cares
                {
                    case "Login":
                        var (chk, localId, pin, region) = await Client.Login(new NationLoginViewModel(currentNation.Name, currentNation.Pass)) ?? default;
                        if(chk != null)
                        {
                            CurrentChk = chk;
                            CurrentPin = pin;

                            FooterText = $"Logged in to {currentNation.Name}.";
                            GetNextButtonText();
                        }
                        else
                        {
                            FooterText = $"Login to {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                        }
                        break;
                    case "Set WFE":
                        if(await Client.SetWFE(currentNation.Region, CurrentChk, CurrentPin, WFE))
                        {
                            FooterText = $"Changed WFE of {currentNation.Region}!";
                            GetNextButtonText();
                        }
                        else
                        {
                            FooterText = $"Setting the WFE of {currentNation.Region} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                            ButtonText = "Login";
                        }
                        break;
                    case "Upload Banner":
                        string? bannerID = await Client.UploadBanner(currentNation.Region, CurrentChk, CurrentPin, BannerPath);
                        if(bannerID != null)
                        {
                            CurrentBannerID = bannerID;
                            FooterText = $"Uploaded banner to {currentNation.Region}!";
                            GetNextButtonText();
                        }
                        else
                        {
                            FooterText = $"Uploading a banner to {currentNation.Region} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                            ButtonText = "Login";
                        }
                        break;
                    case "Upload Flag":
                        string? flagID = await Client.UploadFlag(currentNation.Region, CurrentChk, CurrentPin, FlagPath);
                        if(flagID != null)
                        {
                            CurrentFlagID = flagID;
                            FooterText = $"Uploaded flag to {currentNation.Region}!";
                            GetNextButtonText();
                        }
                        else
                        {
                            FooterText = $"Uploading a flag to {currentNation.Region} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                            ButtonText = "Login";
                        }
                        break;
                    case "Set Banner + Flag":
                        if(await Client.SetBannerFlag(currentNation.Region, CurrentChk, CurrentPin, CurrentBannerID, CurrentFlagID))
                        {
                            FooterText = $"Set banner and flag of {currentNation.Region}!";
                            GetNextButtonText();
                        }
                        else
                        {
                            FooterText = $"Setting banner and flag of {currentNation.Region} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                            ButtonText = "Login";
                        }
                        break;
                    case "Get Embassies":
                        EmbassiesToClose = await Client.GetEmbassies(currentNation.Region, CurrentPin);
                        if(EmbassiesToClose.Count != 0)
                        {
                            FooterText = $"Found embassies in {currentNation.Region}!";
                            ButtonText = "Close Embassy";
                        }
                        else
                        {
                            FooterText = $"Found no embassies to close in {currentNation.Region}.";
                            ButtonText = "Request Embassy";
                        }
                        EmbIndex = 0;
                        break;
                    case "Close Embassy":
                        if(EmbIndex == EmbassiesToClose.Count)
                        {
                            FooterText = "Done closing embassies!";
                            ButtonText = "Request Embassy";
                            EmbIndex = 0;
                        }
                        else
                        {
                            if(await Client.CloseEmbassy(currentNation.Region, CurrentChk, CurrentPin, EmbassiesToClose[EmbIndex]))
                            {
                                FooterText = $"Successfully closed embassy {EmbassiesToClose[EmbIndex]}";
                            }
                            else
                            {
                                FooterText = $"Embassy closure failed, skipping...";
                            }
                            EmbIndex++;
                        }
                        break;
                    case "Request Embassy":
                        if(EmbIndex == _embassyList.Count)
                        {
                            FooterText = "Done requesting embassies!";
                            LoginIndex++;
                            ButtonText = "Login";
                        }
                        else
                        {
                            if(await Client.RequestEmbassy(currentNation.Region, CurrentChk, CurrentPin, _embassyList[EmbIndex]))
                            {
                                FooterText = $"Successfully requested embassies with {_embassyList[EmbIndex]}";
                            }
                            else
                            {
                                FooterText = $"Embassy request failed, skipping...";
                            }
                            EmbIndex++;
                        }
                        break;
                }

                ButtonsEnabled = true;
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
            log.Error($"Tagging with {loginName} failed");
            if(FailedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                FailedLogins.Append('\n');
            }
            FailedLogins.Append(loginName + ", ");
        }

        /// <summary>
        /// This switch statement reconciles the logic with the booleans and the ButtonText field to progress
        /// the button to its next logical text. Somehow.
        /// </summary>
        private void GetNextButtonText()
        {
            switch(ButtonText)
            {
                case "Login":
                    if(WFEEnabled)
                    {
                        ButtonText = "Set WFE";
                        break;
                    }
                    goto case "Set WFE"; //no default fall through in C#, compiler gets mad
                case "Set WFE":
                    if(FlagBannerEnabled)
                    {
                        ButtonText = "Upload Banner";
                        break;
                    }
                    goto case "Set Banner + Flag";
                case "Upload Banner":
                    ButtonText = "Upload Flag";
                    break;
                case "Upload Flag":
                    ButtonText = "Set Banner + Flag";
                    break;
                case "Set Banner + Flag":
                    if(EmbassiesEnabled)
                    {
                        ButtonText = "Get Embassies";
                        break;
                    }
                    goto default;
                default:
                    ButtonText = "Login";
                    LoginIndex++;
                    break;
            }
        }
    }
}
