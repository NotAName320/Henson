/*
Base View Model
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


using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;
using Henson.Models;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels
{
    public static class DesignData
    {
        public static readonly PrepSelectedWindowViewModel PrepDesignVm =
            new([new NationViewModel(new Nation("Test1", "1", "1", "test region"), true, false, null!)], null!, "",
                Brushes.White, false, Brushes.White, 0.65);

        public static readonly TagSelectedWindowViewModel TagDesignVm =
            new([new NationViewModel(new Nation("Test1", "1", "1", "test region"), true, false, null!)], null!, "",
                Brushes.White, false, Brushes.White, 0.65);
    }
    
    public class ViewModelBase : ReactiveObject
    {
        public static readonly bool IsNotLinux = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        public IBrush BackgroundColor
        {
            get => _backgroundColor;
            set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
        }
        private IBrush _backgroundColor = Brushes.Black;
        
        public bool EnableAcrylic
        {
            get => _enableAcrylic;
            set => this.RaiseAndSetIfChanged(ref _enableAcrylic, value);
        }
        private bool _enableAcrylic = false;

        public IBrush AcrylicTint
        {
            get => _acrylicTint;
            set => this.RaiseAndSetIfChanged(ref _acrylicTint, value);
        }
        private IBrush _acrylicTint = Brushes.Black;

        public double AcrylicOpacity
        {
            get => _acrylicOpacity;
            set => this.RaiseAndSetIfChanged(ref _acrylicOpacity, value);
        }
        private double _acrylicOpacity = 0.65;
        
        public IReadOnlyList<WindowTransparencyLevel> AcrylicTransparency
        {
            get => _acrylicTransparency;
            set => this.RaiseAndSetIfChanged(ref _acrylicTransparency, value);
        }
        private IReadOnlyList<WindowTransparencyLevel> _acrylicTransparency = [];
        
        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();
    }
}
