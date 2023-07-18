/*
View Model representing Filter Nations Window
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


using System.Reactive;
using System.Reactive.Linq;
using MessageBox.Avalonia.DTO;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class FilterNationsWindowViewModel : ViewModelBase
    {
        public string NumNations { get; set; } = "";
        
        public string RegionName { get; set; }

        public int DropdownIndex
        {
            get => _dropdownIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _dropdownIndex, value);
            }
        }
        private int _dropdownIndex = 0;

        public bool WithLocked { get; set; } = false;

        public bool RegionBoxEnabled => _regionBoxEnabled.Value;
        private readonly ObservableAsPropertyHelper<bool> _regionBoxEnabled;

        public ReactiveCommand<Unit, (int, string, bool?, bool)?> FilterCommand { get; }
        
        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        private int? IntNumNations =>
            NumNations == "" ? int.MaxValue : int.TryParse(NumNations, out var i) && i > 0 ? i : null;

        public FilterNationsWindowViewModel(string regionName)
        {
            RegionName = regionName;

            FilterCommand = ReactiveCommand.CreateFromTask<(int, string, bool?, bool)?>(async () =>
            {
                if(IntNumNations != null)
                {
                    return ((int)IntNumNations, RegionBoxEnabled ? RegionName : "",
                        RegionBoxEnabled ? DropdownIndex == 1 : null, WithLocked);
                }
                
                var messageBoxDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "No Number Inputted",
                    ContentMessage = "Please input a valid number.",
                    Icon = Icon.Error
                });
                await MessageBoxDialog.Handle(messageBoxDialog);

                return null;
            });

            this.WhenAnyValue(x => x.DropdownIndex).Select(_ => DropdownIndex != 0)
                .ToProperty(this, x => x.RegionBoxEnabled, out _regionBoxEnabled);
        }
    }
}