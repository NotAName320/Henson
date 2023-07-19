/*
View Model representing Tag Selected Window. This file is a fucking mess!
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


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData.Binding;
using Henson.Models;
using log4net;
using MessageBox.Avalonia.DTO;
using MsBox.Avalonia.Enums;
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
        public string Wfe { get; set; } = "";

        public string? CurrentSelectedTag => (string?)CurrentSelectedItem?.Content;
        public ComboBoxItem? CurrentSelectedItem { get; set; } = null;

        /// <summary>
        /// The full file path to the banner image.
        /// </summary>
        public string BannerPath
        {
            get => _bannerPath;
            set
            {
                _bannerPath = value;
                if(_bannerPath != "" && _bannerPath.Contains('/'))
                {
                    BannerFileName = _bannerPath.Split('/').Last();
                }
                else if(_bannerPath != "" && _bannerPath.Contains('\\'))
                {
                    BannerFileName = _bannerPath.Split('\\').Last();
                }
            }
        }
        private string _bannerPath = "";

        /// <summary>
        /// The full file path to the flag image.
        /// </summary>
        public string FlagPath
        {
            get => _flagPath;
            set
            {
                _flagPath = value;
                if(_flagPath != "" && _flagPath.Contains('/'))
                {
                    FlagFileName = _flagPath.Split('/').Last();
                }
                else if(_flagPath != "" && _flagPath.Contains('\\'))
                {
                    FlagFileName = _flagPath.Split('\\').Last();
                }
            }
        }
        private string _flagPath = "";

        /// <summary>
        /// Fired when the question mark button is clicked on the settings page beside the embassy whitelist text box.
        /// </summary>
        public ICommand EmbassyHelpCommand { get; }
        
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
        /// Fired when the Add/Remove button in the tag row is clicked.
        /// </summary>
        public ICommand AddRemoveTagCommand { get; }

        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// I should really create a common class for these lmao
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        /// <summary>
        /// This interaction opens the a file window, and returns a string array with the first value being the file chosen,
        /// or null if the window is closed without a pick.
        /// </summary>
        public Interaction<ViewModelBase, string?> FilePickerDialog { get; } = new();

        /// <summary>
        /// The text on the button.
        /// </summary>
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref _buttonText, value);
            }
        }
        private string _buttonText = "Login";

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

        public string RegionalTagsSelected => _regionalTagsSelected.Value;
        private readonly ObservableAsPropertyHelper<string> _regionalTagsSelected;

        private static string TagToRequest(string x) => x.Replace(":", "").Replace(' ', '_').ToLower();

        private static readonly HashSet<string> UnremovableTags = new()
        {
            "Commended", "Condemned", "Liberated", "Injuncted", "Minuscule", "Small", "Medium", "Large", "Enormous",
            "Gargantuan", "Featured", "Founderless", "Governorless", "New", "Frontier"
        };

        private List<string>? _optionalTagsDetected = new();

        private List<string> TagsToAdd =>
            _selectedTags.Except(_optionalTagsDetected ?? Array.Empty<string>().ToList()).ToList();
        private List<string>? TagsToRemove => _optionalTagsDetected?.Except(_selectedTags).ToList();

        private List<string>? _rmbPostsToRemove = new();

        public bool EmbassiesEnabled
        {
            get => _embassiesEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _embassiesEnabled, value);
            }
        }
        private bool _embassiesEnabled = false;

        public bool SuppressionEnabled
        {
            get => _suppressionEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _suppressionEnabled, value);
            }
        }
        private bool _suppressionEnabled = false;

        public bool FlagBannerEnabled
        {
            get => _flagBannerEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _flagBannerEnabled, value);
            }
        }
        private bool _flagBannerEnabled = false;

        public bool WfeEnabled
        {
            get => _wFeEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _wFeEnabled, value);
            }
        }
        private bool _wFeEnabled = false;

        public bool TagsEnabled
        {
            get => _tagsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _tagsEnabled, value);
            }
        }
        private bool _tagsEnabled = false;

        /// <summary>
        /// The text displayed in the footer of the window.
        /// </summary>
        public string FooterText
        {
            get => _footerText;
            set
            {
                this.RaiseAndSetIfChanged(ref _footerText, value);
            }
        }
        private string _footerText = "Click Login to start.";

        /// <summary>
        /// Boolean that controls the enabling and disabling of buttons that send requests
        /// to ensure compliance with API.
        /// </summary>
        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _buttonsEnabled, value);
            }
        }
        private bool _buttonsEnabled = true;

        public string BannerFileName
        {
            get => _bannerFileName;
            set
            {
                this.RaiseAndSetIfChanged(ref _bannerFileName, value);
            }
        }
        private string _bannerFileName = "";

        public string FlagFileName
        {
            get => _flagFileName;
            set
            {
                this.RaiseAndSetIfChanged(ref _flagFileName, value);
            }
        }
        private string _flagFileName = "";

        /// <summary>
        /// The current index that the user is on.
        /// </summary>
        public int LoginIndex
        {
            get => _loginIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _loginIndex, value);
            }
        }
        private int _loginIndex = 0;
        
        /// <summary>
        /// The amount of successful tags.
        /// </summary>
        private int _successIndex = 0;

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private readonly NsClient _client;

        /// <summary>
        /// The list of nations with RO perms to tag.
        /// </summary>
        private readonly List<NationGridViewModel> _nationsToTag;

        /// <summary>
        /// The index that the user is on for a specific subtask.
        /// </summary>
        private int _subIndex = 0;

        /// <summary>
        /// The current chk of the logged in nation.
        /// </summary>
        private string _currentChk = "";

        /// <summary>
        /// The current Pin of the logged in nation.
        /// </summary>
        private string _currentPin = "";

        /// <summary>
        /// The current Banner ID uploaded to the region.
        /// </summary>
        private string _currentBannerId = "";

        /// <summary>
        /// The current Flag ID uploaded to the region.
        /// </summary>
        private string _currentFlagId = "";

        /// <summary>
        /// The names of all the logins that failed.
        /// </summary>
        private readonly StringBuilder _failedLogins = new();

        /// <summary>
        /// Links to all of the tags that succeeded.
        /// </summary>
        private readonly StringBuilder _successfulTags = new();

        /// <summary>
        /// A list of embassies to close.
        /// </summary>
        private List<(string name, int type)>? _embassiesToClose = new();

        private HashSet<string>? AlreadyEstablishedEmbassies => _embassiesToClose
            ?.Select(x => x.name.ToLower().Replace('_', ' '))
            .Intersect(_embassyList.Select(x => x.Trim().ToLower().Replace('_', ' '))).ToHashSet();

        private HashSet<string> _whitelist;

        private ObservableCollection<string> _selectedTags = new();

        /// <summary>
        /// The log4net logger. It will emit messages as from TagSelectedWindowViewModel.
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// This is not supposed to be used and only so that my damn previewer works again
        /// </summary>
        public TagSelectedWindowViewModel() : this(new List<NationGridViewModel>(), new NsClient(), "") { }
        
        public TagSelectedWindowViewModel(List<NationGridViewModel> nations, NsClient client, string whitelist)
        {
            _nationsToTag = nations;
            _client = client;
            _whitelist = whitelist.Split(',').Select(x => x.Trim()).ToHashSet();
            
            EmbassyHelpCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var messageDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "Embassies",
                    ContentMessage = "Enter regions separated by commas\n(e.g. Red Front, Ijaka, Agheasma).",
                    Icon = Icon.Info,
                });
                await MessageBoxDialog.Handle(messageDialog);
            });

            BannerPickerCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                var dialog = new ViewModelBase();
                var result = await FilePickerDialog.Handle(dialog);

                if(result != null) BannerPath = Uri.UnescapeDataString(result);
            });

            FlagPickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new ViewModelBase();
                var result = await FilePickerDialog.Handle(dialog);

                if(result != null) FlagPath = Uri.UnescapeDataString(result);
            });

            AddRemoveTagCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(CurrentSelectedTag == null)
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Nothing Selected",
                        ContentMessage = "Please select a tag to add/remove.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                if(_selectedTags.Contains(CurrentSelectedTag))
                {
                    _selectedTags.Remove(CurrentSelectedTag);
                }
                else
                {
                    _selectedTags.Add(CurrentSelectedTag);
                }
            });

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                //if none of the tag features are enabled
                if(new[] { EmbassiesEnabled, FlagBannerEnabled, WfeEnabled, TagsEnabled, SuppressionEnabled }.All(x => !x))
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Nothing Selected",
                        ContentMessage = "Please select something to do to the regions.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;

                }

                if(WfeEnabled && Wfe == "")
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "No WFE",
                        ContentMessage = "Please set a WFE to tag regions with.",
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
                        ContentMessage = "Please upload both a banner/flag to tag regions with.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                if(LoginIndex == _nationsToTag.Count)
                {
                    FooterText = $"Tagging complete! {_successIndex}/{_nationsToTag.Count} regions successfully tagged!";
                    
                    string messageContent;
                    if(_successIndex != _nationsToTag.Count && _failedLogins.Length != 0)
                    {
                        _failedLogins.Remove(_failedLogins.Length - 2, 2);
                        messageContent =
                            $"All regions have been tagged. Please close the window now. Tagging with the following failed:\n{_failedLogins}";
                    }
                    else
                    {
                        messageContent = "All regions have been tagged. Please close the window now.";
                    }

                    if(_successfulTags.Length != 0)
                    {
                        var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
                        Directory.CreateDirectory(Path.Combine(workingPath, "taglogs"));
                        var path = Path.Combine(workingPath, "taglogs", $"{DateTime.Now:MM-dd-yyyy HHmmss} Tags.txt");
                        File.WriteAllText(path, _successfulTags.ToString());
                    }
                    
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Logins Complete",
                        ContentMessage = messageContent,
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationGridViewModel currentNation = _nationsToTag[LoginIndex];

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(ButtonText) //very dumb mindless code who cares
                {
                    case "Login":
                        var (chk, _, pin, _) =
                            await _client.Login(new NationLoginViewModel(currentNation.Name, currentNation.Pass)) ??
                            default;
                        if(chk != null)
                        {
                            _currentChk = chk;
                            _currentPin = pin;

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
                        if(await _client.SetWfe(currentNation.Region, _currentChk, _currentPin, Wfe))
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
                        var bannerId =
                            await _client.UploadBanner(currentNation.Region, _currentChk, _currentPin, BannerPath);
                        if(bannerId != null)
                        {
                            _currentBannerId = bannerId;
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
                        var flagId = await _client.UploadFlag(currentNation.Region, _currentChk, _currentPin, FlagPath);
                        if(flagId != null)
                        {
                            _currentFlagId = flagId;
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
                        if(await _client.SetBannerFlag(currentNation.Region, _currentChk, _currentPin, _currentBannerId,
                               _currentFlagId))
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
                        _embassiesToClose = await _client.GetEmbassies(currentNation.Region);
                        if(_embassiesToClose != null && _embassiesToClose.Count != 0)
                        {
                            FooterText = $"Found embassies in {currentNation.Region}!";
                            _embassiesToClose.RemoveAll(x =>
                                (_whitelist.Contains(x.name.ToLower().Replace('_', ' ')) ||
                                 _embassyList.Select(y => y.Trim()).Contains(x.name.ToLower().Replace('_', ' '))) ^
                                x.type == 4 || x.type == -1);
                            ButtonText = "Close Embassy";
                        }
                        else
                        {
                            FooterText = $"Found no embassies to close in {currentNation.Region}.";
                            ButtonText = "Request Embassy";
                            _subIndex = 0;
                        }
                        _subIndex = 0;
                        break;
                    case "Close Embassy":
                        if(_subIndex == _embassiesToClose!.Count)
                        {
                            FooterText = "Done closing embassies!";
                            ButtonText = "Request Embassy";
                            _subIndex = 0;
                            break;
                        }
                        
                        var embassyToClose = _embassiesToClose[_subIndex++];
                        if(await _client.CloseEmbassy(currentNation.Region, _currentChk, _currentPin, 
                               embassyToClose.name, embassyToClose.type))
                        {
                            FooterText = embassyToClose.type == 4
                                ? $"Successfully cancelled closure of embassy with {embassyToClose.name}"
                                : $"Successfully closed embassy with {embassyToClose.name}";
                        }
                        else
                        {
                            FooterText = $"Closing embassy with {embassyToClose.name} failed.";
                        }
                        break;
                    case "Request Embassy":
                        if(_subIndex >= _embassyList.Count)
                        {
                            FooterText = "Done requesting embassies!";

                            if(TagsEnabled)
                            {
                                ButtonText = "Get Tags";
                            }
                            else if(SuppressionEnabled)
                            {
                                ButtonText = "Get RMB";
                            }
                            else
                            {
                                AddToSuccessfulTags(currentNation.Region);
                                LoginIndex++;
                                _successIndex++;
                                ButtonText = "Login";
                            }
                        }
                        else
                        {
                            var embassy = _embassyList[_subIndex++].Trim();
                            if(AlreadyEstablishedEmbassies!.Contains(embassy))
                            {
                                FooterText =
                                    $"Already established embassy with {embassy}, skipping...";
                            }
                            else if(await _client.RequestEmbassy(currentNation.Region, _currentChk, _currentPin, embassy))
                            {
                                FooterText = $"Successfully requested embassies with {embassy}.";
                            }
                            else
                            {
                                FooterText = "Embassy request failed/already done, skipping...";
                            }
                        }
                        break;
                    case "Get Tags":
                        _optionalTagsDetected = (await _client.GetTags(currentNation.Region))?.Except(UnremovableTags)
                            .ToList();
                        _subIndex = 0;
                        if(_optionalTagsDetected != null && TagsToRemove!.Count != 0)
                        {
                            ButtonText = "Remove Tag";
                            FooterText = $"Found tags to remove in {currentNation.Region}!";
                        }
                        else
                        {
                            if(TagsToAdd.Count != 0)
                            {
                                ButtonText = "Add Tag";
                                FooterText = "No tags to remove... adding tags now.";
                            }
                            else
                            {
                                AddToSuccessfulTags(currentNation.Region);
                                LoginIndex++;
                                _successIndex++;
                                ButtonText = "Login";
                                FooterText = "No tags to remove/add...";
                            }
                        }
                        break;
                    case "Remove Tag":
                        if(_subIndex == TagsToRemove!.Count)
                        {
                            if(TagsToAdd.Count != 0)
                            {
                                ButtonText = "Add Tag";
                                FooterText = "Done removing tags! Adding tags now.";
                                _subIndex = 0;
                            }
                            else
                            {
                                FooterText = "Done removing tags! No tags to add...";
                                
                                if(SuppressionEnabled)
                                {
                                    ButtonText = "Get RMB";
                                }
                                else
                                {
                                    AddToSuccessfulTags(currentNation.Region);
                                    LoginIndex++;
                                    _successIndex++;
                                    ButtonText = "Login";
                                }
                            }
                        }
                        else
                        {
                            var tag = TagsToRemove[_subIndex++];
                            if(await _client.RemoveTag(currentNation.Region, _currentChk, _currentPin, TagToRequest(tag)))
                            {
                                FooterText = $"Successfully removed tag {tag}";
                            }
                            else
                            {
                                FooterText = $"Failed to remove tag {tag}";
                            }
                        }
                        break;
                    case "Add Tag":
                    {
                        if(_subIndex == TagsToAdd.Count)
                        {
                            FooterText = "Done adding tags!";
                            
                            if(SuppressionEnabled)
                            {
                                ButtonText = "Get RMB";
                            }
                            else
                            {
                                AddToSuccessfulTags(currentNation.Region);
                                LoginIndex++;
                                _successIndex++;
                                ButtonText = "Login";
                            }
                        }
                        else
                        {
                            var tag = TagsToAdd[_subIndex++];
                            if(await _client.AddTag(currentNation.Region, _currentChk, _currentPin, TagToRequest(tag)))
                            {
                                FooterText = $"Successfully added tag {tag}";
                            }
                            else
                            {
                                FooterText = $"Failed to add tag {tag}";
                            }
                        }
                        break;
                    }
                    case "Get RMB":
                    {
                        _rmbPostsToRemove = await _client.GetRmbPostIds(currentNation.Region);
                        _subIndex = 0;

                        if(_rmbPostsToRemove == null || _rmbPostsToRemove.Count == 0)
                        {
                            AddToSuccessfulTags(currentNation.Region);
                            LoginIndex++;
                            _successIndex++;
                            ButtonText = "Login";
                            FooterText = "No RMB posts to suppress...";
                        }
                        else
                        {
                            ButtonText = "Suppress Post";
                            FooterText = $"Found posts to suppress in {currentNation.Region}!";
                        }
                        break;
                    }
                    case "Suppress Post":
                    {
                        if(_subIndex == _rmbPostsToRemove!.Count)
                        {
                            AddToSuccessfulTags(currentNation.Region);
                            LoginIndex++;
                            _successIndex++;
                            ButtonText = "Login";
                            FooterText = "Done suppressing posts!";
                        }
                        else
                        {
                            if(await _client.SuppressRmbPost(currentNation.Region, _currentPin,
                                   _rmbPostsToRemove[_subIndex]))
                            {
                                FooterText =
                                    $"Suppressed post {_rmbPostsToRemove[_subIndex++]} in {currentNation.Region}!";
                            }
                            else
                            {
                                FooterText = $"Suppressing post {_rmbPostsToRemove[_subIndex++]} in {currentNation.Region} failed.";
                            }
                        }
                        break;
                    }
                }

                ButtonsEnabled = true;
            });

            this.WhenAnyValue(x => x.LoginIndex)
                .Select(_ => LoginIndex == _nationsToTag.Count ? "" : _nationsToTag[LoginIndex].Name)
                .ToProperty(this, x => x.CurrentNation, out _currentNation);

            this.WhenAnyValue(x => x.LoginIndex)
                .Select(_ => LoginIndex == _nationsToTag.Count ? "" : _nationsToTag[LoginIndex].Region)
                .ToProperty(this, x => x.CurrentRegion, out _currentRegion);

            _selectedTags.ToObservableChangeSet().Select(_ =>
                    _selectedTags.Count == 0 ? "" : "Current tags: " + string.Join(", ", _selectedTags))
                .ToProperty(this, x => x.RegionalTagsSelected, out _regionalTagsSelected);
        }

        /// <summary>
        /// Adds a name to the failed logins and wraps the string around if necessary.
        /// </summary>
        /// <param name="loginName">The login name to be added.</param>
        private void AddToFailedLogins(string loginName)
        {
            Log.Error($"Tagging with {loginName} failed");
            if(_failedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                _failedLogins.Append('\n');
            }
            _failedLogins.Append(loginName + ", ");
        }

        private void AddToSuccessfulTags(string regionName)
        {
            _successfulTags.Append($"https://www.nationstates.net/region={regionName.Replace(' ', '_').ToLower()}\n");
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
                    if(WfeEnabled)
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
                    if(TagsEnabled)
                    {
                        ButtonText = "Get Tags";
                        break;
                    }

                    if(SuppressionEnabled)
                    {
                        ButtonText = "Get RMB";
                        break;
                    }
                    ButtonText = "Login";
                    AddToSuccessfulTags(CurrentRegion);
                    LoginIndex++;
                    _successIndex++;
                    break;
            }
        }
    }
}
