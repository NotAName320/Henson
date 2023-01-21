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
                    foreach(NationLoginViewModel n in result)
                    {
                        //System.Diagnostics.Debug.WriteLine(n.Name + " " + n.Pass);
                        Nations.Add(new NationGridViewModel(new Nation("Placeholder", "Placeholder", "Placeholder", "Placeholder"), true));
                    }
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
                }
            });
        }

        private async void LoadNations()
        {
            var nations = new List<NationGridViewModel>
            {
                new NationGridViewModel(new Nation("a", "a", "a", "a"), true),
                new NationGridViewModel(new Nation("a", "a", "a", "a"), false),
                new NationGridViewModel(new Nation("a", "a", "a", "a"), false),
                new NationGridViewModel(new Nation("a", "a", "a", "a"), false),
                new NationGridViewModel(new Nation("a", "a", "a", "a"), false),
            };

            foreach(var n in nations)
            {
                Nations.Add(n);
            }
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();

        public ICommand AddNationCommand { get; }
        public ICommand RemoveNationCommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new();
        public Interaction<MessageBoxViewModel, ButtonResult> RemoveNationConfirmationDialog { get; } = new();

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
