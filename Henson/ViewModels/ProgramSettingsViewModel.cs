/*
View Model representing program settings
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


using ReactiveUI;

namespace Henson.ViewModels
{
    public class ProgramSettingsViewModel : ViewModelBase
    {
        public string UserAgent
        {
            get => _userAgent;
            set
            {
                this.RaiseAndSetIfChanged(ref _userAgent, value);
            }
        }
        private string _userAgent = "";

        public int Theme
        {
            get => _theme;
            set
            {
                this.RaiseAndSetIfChanged(ref _theme, value);
            }
        }
        private int _theme = 0;

        public string EmbWhitelist
        {
            get => _embWhitelist;
            set
            {
                this.RaiseAndSetIfChanged(ref _embWhitelist, value);
            }
        }
        private string _embWhitelist = "";
        
        public string JumpPoint
        {
            get => _jumpPoint;
            set
            {
                this.RaiseAndSetIfChanged(ref _jumpPoint, value);
            }
        }
        private string _jumpPoint = "";
    }
}
