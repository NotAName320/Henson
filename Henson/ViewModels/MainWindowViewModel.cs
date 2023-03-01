using Henson.Models;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MainWindowViewModel()
        {
            Settings = LoadSettings();
            SetSettings();

            DbClient.CreateDbIfNotExists();

            RxApp.MainThreadScheduler.Schedule(LoadNations);

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                var dialog = new AddNationWindowViewModel();
                var result = await AddNationDialog.Handle(dialog);

                if(result != null)
                {
                    FooterText = "Loading nations... this may take a while.";
                    await Task.Delay(100);

                    var (nations, authFailedOnSome) = Client.AuthenticateAndReturnInfo(result);
                    foreach(Nation n in nations)
                    {
                        if(Nations.Select(x => x.Name).Contains(n.Name)) continue;

                        Nations.Add(new NationGridViewModel(n, true, this));
                        DbClient.InsertNation(n);
                    }

                    if(authFailedOnSome)
                    {
                        var messageDialog = new MessageBoxViewModel();
                        await SomeNationsFailedToAddDialog.Handle(messageDialog);
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
                    FooterText = "Finished loading!";
                }
            });

            RemoveSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new MessageBoxViewModel();
                var result = await RemoveNationConfirmationDialog.Handle(dialog);

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
                var dialog = new MessageBoxViewModel();
                var selectedNations = Nations.Where(x => x.Checked).ToList();
                var nationLogins = selectedNations.Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();

                FooterText = "Pinging nations...";
                await Task.Delay(100);

                var successes = Client.PingMany(nationLogins);
                if(!successes.All(x => x))
                {
                    for(int i = 0; i < selectedNations.Count; i++)
                    {
                        selectedNations[i].Checked = successes[i];
                    }
                    
                    FooterText = "Nations pinged (some failed)!";

                    await SomeNationsFailedToPingDialog.Handle(dialog);
                }
                else
                {
                    FooterText = "Nations pinged!";

                    await NationPingSuccessDialog.Handle(dialog);
                }
            });

            FindWACommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                FooterText = "Finding WA nation...";
                await Task.Delay(100);

                var result = Client.FindWA(Nations.ToList());

                if(result != null)
                {
                    FooterText = $"WA nation found: {result}";

                    var dialog = new FindWASuccessViewModel(result);
                    await FindWASuccessDialog.Handle(dialog);
                }
                else
                {
                    FooterText = $"WA nation not found.";

                    var dialog = new MessageBoxViewModel();
                    await WANotFoundDialog.Handle(dialog);
                }
            });
        }

        private void LoadNations()
        {
            var nations = DbClient.GetNations();

            foreach(var n in nations)
            {
                Nations.Add(new NationGridViewModel(n, false, this));
            }
        }

        private static ProgramSettingsViewModel LoadSettings()
        {
            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");

            if(!File.Exists(path))
            {
                File.WriteAllText(path, "user_agent = \"\"");
            }

            string setTomlString = File.ReadAllText(path);
            return Toml.ToModel<ProgramSettingsViewModel>(setTomlString); //This will work for now-later, find solution for interversion compatibility
        }

        private void SetSettings()
        {
            Client.UserAgent = Settings.UserAgent;
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();
        public ProgramSettingsViewModel Settings { get; set; }
        public NsClient Client { get; } = new();

        private string currentLoginUser = "";
        public string CurrentLoginUser
        {
            get => currentLoginUser;
            set => this.RaiseAndSetIfChanged(ref currentLoginUser, value);
        }

        private string? currentLocalID = null;

        private string footerText = "Welcome to Henson!";
        public string FooterText
        {
            get => footerText;
            set => this.RaiseAndSetIfChanged(ref footerText, value);
        }
        
        public ICommand AddNationCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand PingSelectedCommand { get; }
        public ICommand FindWACommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new(); //I will simplify all of this eventually
        public Interaction<MessageBoxViewModel, ButtonResult> RemoveNationConfirmationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToAddDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToPingDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> NationPingSuccessDialog { get; } = new();
        public Interaction<FindWASuccessViewModel, ButtonResult> FindWASuccessDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> WANotFoundDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> LoginFailedDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> NotCurrentLoginDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> WAApplicationFailedDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> LocalIDNotFoundDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> LocalIDNeededDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> MoveRegionFailedDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> UserAgentNotSetDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> TargetRegionNotSetDialog { get; } = new();

        public void OnSelectNationsClick()
        {
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.Checked);
            foreach(var nation in Nations)
            {
                nation.Checked = OppositeAllTrueOrFalse;
            }
        }

        public void OnSaveSettingsClick()
        {
            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");
            File.WriteAllText(path, Toml.FromModel(Settings));

            SetSettings();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            FooterText = $"Settings updated.";
        }

        public async Task OnNationLoginClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            var nationLogin = new NationLoginViewModel(nation.Name, nation.Pass);

            FooterText = $"Logging in to {nation.Name}...";
            await Task.Delay(100);

            var result = Client.Login(nationLogin);
            if (result != null)
            {
                nation.Chk = result;

                CurrentLoginUser = nation.Name;
                currentLocalID = null;
                FooterText = $"Logged in to {nation.Name}";
            }
            else
            {
                FooterText = $"Failed to log in to {nation.Name}";
                await Task.Delay(100);

                var dialog = new MessageBoxViewModel();
                await LoginFailedDialog.Handle(dialog);
            }
        }

        private async Task<bool> NationEqualsLogin(NationGridViewModel nation)
        {
            if(nation.Name != currentLoginUser)
            {
                var dialog = new MessageBoxViewModel();
                await NotCurrentLoginDialog.Handle(dialog);
                return false;
            }
            return true;
        }

        private async Task<bool> UserAgentNotSet()
        {
            if(Settings.UserAgent == "")
            {
                var dialog = new MessageBoxViewModel();
                await UserAgentNotSetDialog.Handle(dialog);
                return true;
            }
            return false;
        }

        public async Task OnNationApplyWAClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            var chk = nation.Chk!;

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

                var dialog = new MessageBoxViewModel();
                await WAApplicationFailedDialog.Handle(dialog);
            }
        }

        public async Task OnNationGetLocalIDClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            var localID = Client.GetLocalID();

            if(localID != null)
            {
                currentLocalID = localID;
                FooterText = $"Got local ID of {nation.Name}, ready to move regions!";
            }
            else
            {
                CurrentLoginUser = "";
                currentLocalID = null;
                FooterText = $"Getting local ID failed... please log in again.";
                await Task.Delay(100);

                var dialog = new MessageBoxViewModel();
                await LocalIDNotFoundDialog.Handle(dialog);
            }
        }

        public async Task OnNationMoveRegionClick(NationGridViewModel nation, string region)
        {
            var dialog = new MessageBoxViewModel();

            if (await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            if(region == null)
            {
                await TargetRegionNotSetDialog.Handle(dialog);
                return;
            }
            
            if(currentLocalID == null)
            {
                await LocalIDNeededDialog.Handle(dialog);
                return;
            }

            FooterText = $"Moving {nation.Name} to {region}... this may take a while.";
            await Task.Delay(100);

            if (Client.MoveToJP(region, currentLocalID))
            {
                FooterText = $"{nation.Name} moved to {region}!";
                nation.Region = char.ToUpper(region[0]) + region[1..];
                DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{region}' WHERE name = '{nation.Name}'");
            }
            else
            {
                FooterText = $"Moving {nation.Name} to region {region} failed!";
                await Task.Delay(100);

                await MoveRegionFailedDialog.Handle(dialog);
            }
        }
    }
}
