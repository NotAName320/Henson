/*
View Model representing Main Window
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


using Avalonia.Themes.Fluent;
using DynamicData;
using DynamicData.Binding;
using Henson.Models;
using log4net;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Octokit;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Tomlyn;

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //commands must be defined in the constructor, i'll still provide some brief documentation here
        /// <summary>
        /// Fired when the Add Nation button in quick view is clicked and opens an AddNationDialog.
        /// </summary>
        public ICommand AddNationCommand { get; }

        /// <summary>
        /// Fired when the Remove button in quick view is clicked.
        /// </summary>
        public ICommand RemoveSelectedCommand { get; }

        /// <summary>
        /// Fired when the Ping button in quick view is clicked.
        /// </summary>
        public ICommand PingSelectedCommand { get; }

        /// <summary>
        /// Fired when the Find WA button in quick view is clicked.
        /// </summary>
        public ICommand FindWACommand { get; }

        /// <summary>
        /// Fired when the Prep button in quick view is clicked.
        /// </summary>
        public ICommand PrepSelectedCommand { get; }

        /// <summary>
        /// Fired when the Tag button in quick view is clicked.
        /// </summary>
        public ICommand TagSelectedCommand { get; }

        /// <summary>
        /// This interaction opens the Add Nation Dialog and returns a list of NationLoginViewModels
        /// representing logins to nations that may or may not be valid.
        /// </summary>
        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new();

        /// <summary>
        /// This interaction opens the Prep Selected window.
        /// </summary>
        public Interaction<PrepSelectedWindowViewModel, Unit> PrepSelectedDialog { get; } = new();

        /// <summary>
        /// This interaction opens the Tag Selected window.
        /// </summary>
        public Interaction<TagSelectedWindowViewModel, Unit> TagSelectedDialog { get; } = new();

        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        /// <summary>
        /// The text displayed in the footer.
        /// </summary>
        private string footerText = "Welcome to Henson!";
        public string FooterText
        {
            get => footerText;
            set => this.RaiseAndSetIfChanged(ref footerText, value);
        }

        /// <summary>
        /// The current logged in user displayed in the quick view.
        /// </summary>
        public string CurrentLoginUser
        {
            get => currentLoginUser;
            set => this.RaiseAndSetIfChanged(ref currentLoginUser, value);
        }
        private string currentLoginUser = "";

        /// <summary>
        /// The target region in the quick view's text box, only used in the code to pass on
        /// to a child window.
        /// </summary>
        public string TargetRegion { get; set; } = "";

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
        /// A boolean representation of whether any nation is selected in Nations at any time.
        /// </summary>
        public bool AnyNationSelected => _anyNationSelected.Value;
        private readonly ObservableAsPropertyHelper<bool> _anyNationSelected;

        /// <summary>
        /// A boolean representation of whether both ButtonsEnabled and AnyNationSelected are true at any time.
        /// </summary>
        public bool NationSelectedAndNoSiteRequests => _nationSelectedAndNoSiteRequests.Value;
        private readonly ObservableAsPropertyHelper<bool> _nationSelectedAndNoSiteRequests;

        private string? currentLocalID = null; //should probably store that in the object or the chk here for constitency

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private NsClient Client { get; } = new();

        /// <summary>
        /// Represents the nations loaded by the program.
        /// </summary>
        private ObservableCollection<NationGridViewModel> Nations { get; } = new();

        /// <summary>
        /// Represents the state of the input in the settings tab and not what current settings are loaded/saved.
        /// </summary>
        private ProgramSettingsViewModel Settings { get; set; }

        /// <summary>
        /// The log4net logger. It will emit messages as from MainWindowViewModel.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Constructs a new <c>MainWindowViewModel</c>.
        /// Note that in Avalonia/WPF, ViewModel constructors function as the window's startup code.
        /// </summary>
        public MainWindowViewModel()
        {
            log.Info($"Starting Henson... Version v{GetType().Assembly.GetName().Version} on platform {RuntimeInformation.RuntimeIdentifier}");
            if(File.Exists("henson.log.1")) File.Delete("henson.log.1"); //delete old log

            Settings = LoadSettings();
            SetSettings();

            RxApp.MainThreadScheduler.Schedule(CheckIfLatestRelease);

            DbClient.CreateDbIfNotExists();

            RxApp.MainThreadScheduler.Schedule(LoadNations);

            RxApp.MainThreadScheduler.Schedule(CheckIfUserAgentEmpty);

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                var dialog = new AddNationWindowViewModel();
                var result = await AddNationDialog.Handle(dialog);

                if(result != null)
                {
                    FooterText = "Loading nations... this may take a while.";
                    ButtonsEnabled = false;
                    await Task.Delay(100);

                    var nations = await Client.RunMany(result, Client.Ping);

                    if(nations.Any(x => x == null))
                    {
                        var messageBoxDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                        {
                            ContentTitle = "Warning",
                            ContentMessage = "One or more nation(s) failed to add, probably due to an invalid username/password combo.",
                            Icon = Icon.Warning,
                        });
                        await MessageBoxDialog.Handle(messageBoxDialog);
                        nations = nations.Where(x => x != null).ToList();
                    }

                    foreach(var n in nations)
                    {
                        if(Nations.Select(x => x.Name).Contains(n!.Name)) continue;

                        Nations.Add(new NationGridViewModel(n, true, false, this));
                        DbClient.InsertNation(n);
                    }

                    if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
                    ButtonsEnabled = true;
                    FooterText = "Finished loading!";
                }
            });

            RemoveSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "Remove Selected Nations",
                    ContentMessage = "Are you sure you want to remove the selected nations' logins from Henson?",
                    Icon = Icon.Info,
                    ButtonDefinitions = ButtonEnum.YesNo,
                });
                var result = await MessageBoxDialog.Handle(dialog);

                if(result == ButtonResult.Yes)
                {
                    for(int i = Nations.Count - 1; i >= 0; i--)
                    {
                        if(Nations[i].Checked)
                        {
                            DbClient.DeleteNation(Nations[i].Name);
                            Nations.RemoveAt(i);
                        }
                    }

                    FooterText = "Nations removed!";
                }
            });

            PingSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                
                var selectedNations = Nations.Where(x => x.Checked).ToList();

                var nationLogins = selectedNations.Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();

                FooterText = "Pinging nations...";
                ButtonsEnabled = false;
                await Task.Delay(100);

                var nations = await Client.RunMany(nationLogins, Client.Ping);
                foreach(var n in nations)
                {
                    if(n == null) continue;

                    //There has to be an easier way to do this
                    int index = Nations.IndexOf(Nations.Where(x => x.Name.ToLower() == n.Name.ToLower()).First());
                    DbClient.ExecuteNonQuery("UPDATE nations SET " +
                        $"name = '{n.Name}', " +
                        $"region = '{n.Region}', " +
                        $"flagUrl = '{n.FlagUrl}' " +
                        $"WHERE name = '{Nations[index].Name}'");

                    Nations[index] = new NationGridViewModel(n, true, Nations[index].Locked, this);
                }

                if(nations.Any(x => x == null))
                {
                    for(int i = 0; i < selectedNations.Count; i++)
                    {
                        selectedNations[i].Checked = nations[i] == null;
                    }
                    
                    FooterText = "Nations pinged (some failed)!";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "Warning",
                        ContentMessage = "One or more nation(s) failed to ping, probably due to an invalid username/password combo. " +
                        "They have been selected.",
                        Icon = Icon.Warning,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
                else
                {
                    FooterText = "Nations pinged!";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "Success",
                        ContentMessage = "All nations pinged successfully.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
                ButtonsEnabled = true;
            });

            FindWACommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                FooterText = "Finding WA nation...";
                ButtonsEnabled = false;
                await Task.Delay(100);

                var result = await Client.FindWA(Nations.ToList());

                ButtonsEnabled = true;

                if(result != null)
                {
                    FooterText = $"WA nation found: {result}";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "WA Nation Found",
                        ContentMessage = $"Your WA nation is {result}.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
                else
                {
                    FooterText = $"WA nation not found.";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "WA Nation Not Found",
                        ContentMessage = "Your WA nation was not found.",
                        Icon = Icon.Warning,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
            });

            PrepSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;

                if(!Nations.Any(x => !x.Locked))
                {
                    var messageDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "No Nations Selected",
                        ContentMessage = "Please select some (unlocked) nations first.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(messageDialog);
                    return;
                }

                FooterText = "Opening prep window...";
                await Task.Delay(100);

                var dialog = new PrepSelectedWindowViewModel(Nations.ToList(), Client, TargetRegion);
                await PrepSelectedDialog.Handle(dialog);

                foreach(var n in Nations)
                {
                    DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{n.Region}' WHERE name = '{n.Name}'");
                }

                FooterText = "Nations prepped!";
            });

            TagSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;

                if(!Nations.Any(x => !x.Locked))
                {
                    var messageDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "No Nations Selected",
                        ContentMessage = "Please select some (unlocked) nations first.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(messageDialog);
                    return;
                }

                FooterText = "Checking which nations have taggable RO perms...";
                await Task.Delay(100);

                var SelectedNations = Nations.Where(x => x.Checked && !x.Locked).ToList();
                var TaggableNations = (await Client.RunMany(SelectedNations, Client.IsROWithTagPerms)).Where(x => x != null).ToList();

                if(TaggableNations.Count == 0)
                {
                    var messageDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "No Taggable Regions",
                        ContentMessage = "None of the selected nations were in regions they could tag.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(messageDialog);
                    return;
                }

                var dialog = new TagSelectedWindowViewModel(Nations.ToList(), Client, TargetRegion);
                await TagSelectedDialog.Handle(dialog);
            });

            //When any nation is checked or unchecked see if any nation is checked at all and set that value to a property
            Nations.ToObservableChangeSet().AutoRefresh(x => x.Checked).ToCollection()
                   .Select(x => x.Any(y => y.Checked)).ToProperty(this, x => x.AnyNationSelected, out _anyNationSelected);

            //Combine ButtonsEnabled property with AnyNationSelected property to get property
            //for buttons that need to be disabled in both situations
            this.WhenAnyValue(x => x.ButtonsEnabled, x => x.AnyNationSelected).Select(_ => ButtonsEnabled && AnyNationSelected)
                .ToProperty(this, x => x.NationSelectedAndNoSiteRequests, out _nationSelectedAndNoSiteRequests);
        }

        /// <summary>
        /// Locks selected nations, or unlocks if all selected nations are locked.
        /// </summary>
        /// <returns></returns>
        public async Task OnLockSelectedClick()
        {
            var selectedNations = Nations.Where(x => x.Checked).ToList();
            if(selectedNations.Count == 0)
            {
                var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "No Nations Selected",
                    ContentMessage = "Please select some nations first.",
                    Icon = Icon.Info,
                });
                await MessageBoxDialog.Handle(dialog);
                return;
            }

            bool OppositeAllTrueOrFalse = !selectedNations.All(x => x.Locked);
            foreach(var nation in selectedNations)
            {
                nation.Locked = OppositeAllTrueOrFalse;
                DbClient.ExecuteNonQuery($"UPDATE nations SET locked = {nation.Locked} WHERE name = '{nation.Name}'");
            }
            FooterText = "Selected nation(s) " + (OppositeAllTrueOrFalse ? "locked!" : "unlocked!");
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
        }

        /// <summary>
        /// Fired when Select/Unselect all is clicked in the quick view menu.
        /// </summary>
        public void OnSelectNationsClick()
        {
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.Checked);
            foreach(var nation in Nations)
            {
                nation.Checked = OppositeAllTrueOrFalse;
            }
        }

        /// <summary>
        /// Fired when Save is clicked in the settings menu.
        /// </summary>
        public void OnSaveSettingsClick()
        {
            var model = Toml.ToModel("");

            model["user_agent"] = Settings.UserAgent;
            model["theme"] = Settings.Theme == 1 ? "dark" : "light";

            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");
            File.WriteAllText(path, Toml.FromModel(model));

            SetSettings();
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            FooterText = $"Settings updated.";

            //easter egg :)
            Process process = new();
            process.StartInfo.UseShellExecute = true;
            if(Settings.UserAgent == "092436")
            {
                process.StartInfo.FileName = "https://www.youtube.com/watch?v=WS3Lkc6Gzlk";
                process.Start();
            }
            if(Settings.UserAgent == "051690")
            {
                process.StartInfo.FileName = "https://www.youtube.com/watch?v=57ta7mkgrOU";
                process.Start();
            }
        }

        /// <summary>
        /// Fired when Login is clicked in the quick view grid.
        /// </summary>
        /// <param name="nation">The nation on which Login is being clicked.</param>
        /// <returns></returns>
        public async Task OnNationLoginClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            var nationLogin = new NationLoginViewModel(nation.Name, nation.Pass);

            FooterText = $"Logging in to {nation.Name}...";
            ButtonsEnabled = false;
            await Task.Delay(100);

            var (chk, localId) = Client.Login(nationLogin) ?? default;
            if(chk != null)
            {
                nation.Chk = chk;
                currentLocalID = localId;
                CurrentLoginUser = nation.Name;

                FooterText = $"Logged in to {nation.Name}";
            }
            else
            {
                CurrentLoginUser = "";
                FooterText = $"Failed to log in to {nation.Name}";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Login Failed",
                    ContentMessage = "The login failed, probably due to an invalid username/password combination.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
            }
            ButtonsEnabled = true;
        }

        /// <summary>
        /// Fired when Apply WA is clicked in the quick view grid.
        /// </summary>
        /// <param name="nation">The nation on which Apply WA is being clicked.</param>
        /// <returns></returns>
        public async Task OnNationApplyWAClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            if(nation.Locked)
            {
                FooterText = "Nation is locked!";
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
                return;
            }

            var chk = nation.Chk!;

            ButtonsEnabled = false;
            await Task.Delay(100);
            if(Client.ApplyWA(chk))
            {
                FooterText = $"{nation.Name} WA application successful!";
            }
            else
            {
                CurrentLoginUser = "";
                currentLocalID = null;
                FooterText = $"{nation.Name} WA application failed... please log in again.";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "WA Application Failed",
                    ContentMessage = "Please log in again.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
            }
            ButtonsEnabled = true;
        }

        /// <summary>
        /// Fired when Move is clicked on a nation in the quick view grid.
        /// </summary>
        /// <param name="nation">The nation on which Move is being clicked.</param>
        /// <param name="region">The target region in the text box when Move was clicked.</param>
        /// <returns></returns>
        public async Task OnNationMoveRegionClick(NationGridViewModel nation, string region)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            if(region == "")
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Target Region Not Set",
                    ContentMessage = "Please set a target region.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
                return;
            }

            if(region.ToLower() == nation.Region.ToLower())
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Nation Already In Region",
                    ContentMessage = $"Your nation is already in the region {nation.Region}.",
                    Icon = Icon.Info,
                });

                await MessageBoxDialog.Handle(dialog);
                return;
            }

            if(nation.Locked)
            {
                FooterText = "Nation is locked!";
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
                return;
            }

            FooterText = $"Moving {nation.Name} to {region}... this may take a while.";
            ButtonsEnabled = false;
            await Task.Delay(100);

            if(Client.MoveToJP(region, currentLocalID!))
            {
                FooterText = $"{nation.Name} moved to {region}!";
                nation.Region = char.ToUpper(region[0]) + region[1..];
                DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{region}' WHERE name = '{nation.Name}'");
            }
            else
            {
                FooterText = $"Moving {nation.Name} to region {region} failed!";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Moving Region Failed",
                    ContentMessage = "Moving to the region failed.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
            }
            ButtonsEnabled = true;
        }

        /// <summary>
        /// Checks if the nation provided matches the nation currently logged in, and shows an error
        /// message box if it doesn't.
        /// </summary>
        /// <param name="nation">The nation to be checked with the current login.</param>
        /// <returns>A boolean value representing whether or not the provided nation matches the
        /// current login.</returns>
        private async Task<bool> NationEqualsLogin(NationGridViewModel nation)
        {
            if(nation.Name != currentLoginUser)
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Current Login Doesn't Match",
                    ContentMessage = "Please log in with the the account you are trying to perform this action with.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the user agent has a non-empty value, and shows an error message box if it is.
        /// </summary>
        /// <returns>A boolean value representing whether or not the user agent is empty.</returns>
        private async Task<bool> UserAgentNotSet()
        {
            if(Settings.UserAgent == "")
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "User Agent Not Set",
                    ContentMessage = "Please go to the Settings tab to set the User Agent.",
                    Icon = Icon.Error,
                });
                await MessageBoxDialog.Handle(dialog);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads nations from the database.
        /// </summary>
        private void LoadNations()
        {
            var (nations, locked) = DbClient.GetNations();

            foreach(var n in nations)
            {
                Nations.Add(new NationGridViewModel(n, false, locked.Contains(n.Name), this));
            }
        }

        /// <summary>
        /// Loads settings from the settings file,
        /// </summary>
        /// <returns>A ProgramSettingsViewModel object representing the settings from the loaded file.</returns>
        private static ProgramSettingsViewModel LoadSettings()
        {
            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");

            if(!File.Exists(path))
            {
                File.Create(path).Dispose(); //avoids IOException
            }

            string setTomlString = File.ReadAllText(path);
            var model = Toml.ToModel(setTomlString);

            ProgramSettingsViewModel retVal = new();
            try
            {
                retVal.UserAgent = (string)model["user_agent"];
            }
            catch (KeyNotFoundException) { model["user_agent"] = ""; }

            try
            {
                retVal.Theme = ((string)model["theme"]).ToLower() == "dark" ? 1 : 0;
            }
            catch (KeyNotFoundException) { model["theme"] = "light"; }

            File.WriteAllText(path, Toml.FromModel(model));

            return retVal;
        }
        
        /// <summary>
        /// Changes program variables to match those specified by the current ProgramSettingsViewModel.
        /// </summary>
        private void SetSettings()
        {
            log.Info(Settings.UserAgent == "" ? "User agent set to empty string!" : $"User agent set to {Settings.UserAgent}");
            Client.UserAgent = Settings.UserAgent;

            var theme = (FluentTheme)Avalonia.Application.Current!.Styles[0]; //yes we are fishing blindly for the FluentTheme within Styles
            if(Settings.Theme == 1)
            {
                log.Info("Theme set to Dark");
                theme.Mode = FluentThemeMode.Dark;
            }
            else
            {
                log.Info("Theme set to Light");
                theme.Mode = FluentThemeMode.Light;
            }

            //force the application to reload DataGrid theming otherwise it follows existing theme
            //yes this is stupid
            var uri = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml");
            Avalonia.Application.Current.Styles[1] = new Avalonia.Markup.Xaml.Styling.StyleInclude(uri)
            {
                Source = uri
            };
        }

        /// <summary>
        /// Checks if the version is the latest release on GitHub and shows a notification window (and opens web browser to releases page) if it isn't.
        /// </summary>
        private async void CheckIfLatestRelease()
        {
            var currentVer = GetType().Assembly.GetName().Version!;
            var client = new GitHubClient(new ProductHeaderValue(Uri.EscapeDataString($"NotAName320/Henson v{currentVer}")));
            var latestRelease = await client.Repository.Release.GetLatest("NotAName320", "Henson");

            if(new Version(latestRelease.TagName.Replace("v", "")) <= currentVer) return;

            log.Warn($"Newer version now available! Current version: v{currentVer}, latest version on GitHub: {latestRelease.TagName}");

            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
            {
                ContentTitle = "New Version Available",
                ContentMessage = $"A new version ({latestRelease.TagName}) of Henson is now available.\n\n" +
                "You can get it at https://github.com/NotAName320/Henson/releases.\n\n" +
                "Updating immediately is always recommended to ensure site rules compliance.",
                Icon = Icon.Info,
            });
            await MessageBoxDialog.Handle(dialog);

            //opens web browser, works on windows and linux (IDK about mac)
            Process process = new();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "https://github.com/NotAName320/Henson/releases";
            process.Start();
        }

        /// <summary>
        /// Checks if the user agent is empty and shows a notification window if it is.
        /// </summary>
        private async void CheckIfUserAgentEmpty()
        {
            if(Settings.UserAgent == "")
            {
                log.Warn("User agent has no value!");
                await Task.Delay(100); //Stupidest hack ever, but wait till MessageBoxDialog is initialized before showing because this runs so fast

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "User Agent Empty",
                    ContentMessage = "You must set a user agent in Settings before being able to use the features of Henson.",
                    Icon = Icon.Warning,
                });
                await MessageBoxDialog.Handle(dialog);
            }
        }
    }
}
