﻿using Henson.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class NationGridViewModel : ViewModelBase
    {
        private readonly Nation _nation;

        public NationGridViewModel(Nation nation, bool _checked, MainWindowViewModel parent)
        {
            _nation = nation;
            this._checked = _checked;
            Parent = parent; //This is a dumb solution to a dumb problem: not being able to access parent VMs in a DataGrid.

            Login = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationLoginClick(this);
            });
        }

        public string Name => _nation.Name;
        public string Pass => _nation.Pass;
        public string Region => _nation.Region;
        public (string, string) PinChk { get; set; }

        public ICommand Login { get; }

        public void OnCheckboxClick() //Saved for adding checkbox to header later
        {
        }

        public void ApplyWA()
        {
            System.Diagnostics.Debug.WriteLine("Apply WA " + Name + " " + Pass);
        }

        public void MoveTo(string region)
        {
            System.Diagnostics.Debug.WriteLine("MoveTo " + region);
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }

        public MainWindowViewModel Parent { get; set; }
    }
}