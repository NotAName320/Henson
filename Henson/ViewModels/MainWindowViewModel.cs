using Henson.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

                    var (nations, authFailedOnSome) = _client.AuthenticateAndReturnInfo(result);
                    foreach (Nation n in nations)
                    {
                        Nations.Add(new NationGridViewModel(n, true));
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

            LoginSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new MessageBoxViewModel();
                var selectedNations = Nations.Where(x => x.Checked).ToList();
                var nationLogins = selectedNations.Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();

                FooterText = "Logging nations in...";
                await Task.Delay(100);

                var successes = _client.LoginMany(nationLogins);
                if(!successes.All(x => x))
                {
                    for(int i = 0; i < selectedNations.Count; i++)
                    {
                        selectedNations[i].Checked = successes[i];
                    }
                    
                    FooterText = "Nations logged in (some failed)!";

                    await SomeNationsFailedToLoginDialog.Handle(dialog);
                }
                else
                {
                    FooterText = "Nations logged in!";

                    await NationLoginSuccessDialog.Handle(dialog);
                }
            });

            FindWACommand = ReactiveCommand.CreateFromTask(async () =>
            {
                FooterText = "Finding WA nation...";
                await Task.Delay(100);

                var result = _client.FindWA(Nations.ToList());

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

        private NSClient _client { get; } = new("Notanam");

        private string footerText = "Welcome to Henson!";
        public string FooterText
        {
            get => footerText;
            set => this.RaiseAndSetIfChanged(ref footerText, value);
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();

        public ICommand AddNationCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand LoginSelectedCommand { get; }
        public ICommand FindWACommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> RemoveNationConfirmationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToAddDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToLoginDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> NationLoginSuccessDialog { get; } = new();
        public Interaction<FindWASuccessViewModel, ButtonResult> FindWASuccessDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> WANotFoundDialog { get; } = new();

        public void OnSelectNationsClick()
        {
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.Checked);
            foreach(var nation in Nations)
            {
                nation.Checked = OppositeAllTrueOrFalse;
            }
        }
    }
}
