/*
View Model representing nation in grid
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


using Henson.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class NationGridViewModel : ViewModelBase
    {
        public string Name => _nation.Name;
        public string Pass => _nation.Pass;
        public string Region
        {
            get => _nation.Region;
            set
            {
                if (value == _nation.Region) return;
                _nation.Region = value;
                this.RaisePropertyChanged();
            }
        }
        public string? Chk { get; set; } = null;

        public ICommand Login { get; }
        public ICommand ApplyWA { get; }
        public ReactiveCommand<string, Unit> MoveTo { get; }

        public MainWindowViewModel Parent { get; set; }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }

        private bool _locked;
        public bool Locked
        {
            get => _locked;
            set => this.RaiseAndSetIfChanged(ref _locked, value);
        }

        private readonly ObservableAsPropertyHelper<string> _gridName;
        public string GridName
        {
            get => _gridName.Value;
        }

        private readonly Nation _nation;

        public NationGridViewModel(Nation nation, bool _checked, bool _locked, MainWindowViewModel parent)
        {
            _nation = nation;
            this._checked = _checked;
            this._locked = _locked;
            Parent = parent; //This is a dumb solution to a dumb problem: not being able to access parent VMs in a DataGrid.

            Login = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationLoginClick(this);
            });

            ApplyWA = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationApplyWAClick(this);
            });

            MoveTo = ReactiveCommand.CreateFromTask<string>(async (region) =>
            {
                await Parent.OnNationMoveRegionClick(this, region);
            });

            //dont even ask me to explain this!!! reactiveui is confusing
            //peep this: https://stackoverflow.com/questions/41526936/can-i-avoid-explicitly-calling-raisepropertychanged-for-dependent-properties
            this.WhenAnyValue(x => x.Name, x => x.Locked).Select(_ => Name + (Locked ? "🔒" : ""))
                .ToProperty(this, x => x.GridName, out _gridName);
        }
    }
}
