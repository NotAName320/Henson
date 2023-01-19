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

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadNations);

            ShowAddNationDialog = new Interaction<AddNationWindowViewModel, AddNationOptionViewModel?>();

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new AddNationWindowViewModel();

                var result = await ShowAddNationDialog.Handle(dialog);
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

            foreach(var nation in nations)
            {
                Nations.Add(nation);
            }
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();

        public ICommand AddNationCommand { get; }

        public Interaction<AddNationWindowViewModel, AddNationOptionViewModel?> ShowAddNationDialog { get; }

        public void OnRemoveNationClick()
        {
            System.Diagnostics.Debug.WriteLine("OnRemoveNationClick");
        }

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
