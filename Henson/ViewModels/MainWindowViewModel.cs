using Henson.Models;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Collections.ObjectModel;
using MessageBox.Avalonia.Enums;
using System.Reactive;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadNations);

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new AddNationWindowViewModel();
                var result = await AddNationDialog.Handle(dialog);

                if(result != null)
                {
                    FooterText = "Loading nations... this may take a while.";
                    await Task.Delay(100);

                    var (nations, authFailedOnSome) = Client.AuthenticateAndReturnInfo(result);
                    foreach (Nation n in nations)
                    {
                        Nations.Add(new NationGridViewModel(n, true, this));
                    }

                    if(authFailedOnSome)
                    {
                        var messageDialog = new MessageBoxViewModel();
                        await SomeNationsFailedToAddDialog.Handle(messageDialog);
                    }

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
                            Nations.RemoveAt(i);
                        }
                    }

                    FooterText = "Nations removed!";
                }
            });

            PingSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
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
            var nations = new List<NationGridViewModel>();

            foreach(var n in nations)
            {
                Nations.Add(n);
            }
        }

        public NSClient Client { get; } = new("Notanam");

        private string? currentLoginUser = null;
        public string CurrentLoginUser
        {
            get => currentLoginUser ?? "";
            set => this.RaiseAndSetIfChanged(ref currentLoginUser, value);
        }

        private string footerText = "Welcome to Henson!";
        public string FooterText
        {
            get => footerText;
            set => this.RaiseAndSetIfChanged(ref footerText, value);
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();

        public ICommand AddNationCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand PingSelectedCommand { get; }
        public ICommand FindWACommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> RemoveNationConfirmationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToAddDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToPingDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> NationPingSuccessDialog { get; } = new();
        public Interaction<FindWASuccessViewModel, ButtonResult> FindWASuccessDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> WANotFoundDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> LoginFailedDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> NotCurrentLoginDialog { get; } = new();

        public void OnSelectNationsClick()
        {
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.Checked);
            foreach(var nation in Nations)
            {
                nation.Checked = OppositeAllTrueOrFalse;
            }
        }

        public async Task OnNationLoginClick(NationGridViewModel nation)
        {
            var nationLogin = new NationLoginViewModel(nation.Name, nation.Pass);

            FooterText = $"Logging in to {nation.Name}...";
            await Task.Delay(100);

            var result = Client.Login(nationLogin);
            if (result != null)
            {
                nation.PinChk = result ?? default;

                CurrentLoginUser = nation.Name;
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

        private async Task<bool> nationEqualsLogin(NationGridViewModel nation)
        {
            if(nation.Name != currentLoginUser)
            {
                var dialog = new MessageBoxViewModel();
                await NotCurrentLoginDialog.Handle(dialog);
                return false;
            }
            return true;
        }

        public async Task OnNationApplyWAClick(NationGridViewModel nation)
        {
            if(!await nationEqualsLogin(nation)) return;
        }

        public async Task OnNationGetLocalIDClick(NationGridViewModel nation)
        {
            if(!await nationEqualsLogin(nation)) return;
        }

        public async Task OnNationMoveRegionClick(NationGridViewModel nation, string region)
        {
            if(!await nationEqualsLogin(nation)) return;
        }
    }
}
