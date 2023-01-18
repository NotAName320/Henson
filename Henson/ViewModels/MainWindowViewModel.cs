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

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            ShowDialog = new Interaction<AddNationWindowViewModel, AddNationOptionViewModel?>();

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new AddNationWindowViewModel();

                var result = await ShowDialog.Handle(dialog);
            });
        }

        public List<Nation> Nations => new()
        {
            new Nation("a", "a", "a", "a", true),
            new Nation("b", "b", "b", "b", false),
            new Nation("c", "c", "c", "c", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
            new Nation("d", "d", "d", "d", false),
        };

        public ICommand AddNationCommand { get; }

        public Interaction<AddNationWindowViewModel, AddNationOptionViewModel?> ShowDialog { get; }

        public void OnAddNationClick()
        {
            System.Diagnostics.Debug.WriteLine("OnAddNationClick");
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
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.IsChecked);
            for(int i = 0; i < Nations.Count; i++)
            {
                Console.WriteLine(Nations[i].Name + " changed from "+ Nations[i].IsChecked.ToString() + " to " + OppositeAllTrueOrFalse.ToString());
                Nations[i].IsChecked = OppositeAllTrueOrFalse;
            }
        }
    }
}
