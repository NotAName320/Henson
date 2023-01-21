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
using DynamicData;
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

            RemoveNationCommand = ReactiveCommand.CreateFromTask(async () =>
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
        }

        private async void LoadNations()
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
        public ICommand RemoveNationCommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> RemoveNationConfirmationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> SomeNationsFailedToAddDialog { get; } = new();

        public void OnLoginSelectedClick()
        {
            System.Diagnostics.Debug.WriteLine("OnLoginSelectedClick");
        }

        public void OnFindWAClick()
        {
            System.Diagnostics.Debug.WriteLine("OnFindWAClick");
        }

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
