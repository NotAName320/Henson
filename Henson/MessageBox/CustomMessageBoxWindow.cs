/*
View Model representing Add Folder Window
Copyright (C) 2024 NotAName320

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


using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Henson.MessageBox;

public class CustomMessageBoxWindow : MsBox.Avalonia.Windows.MsBoxWindow
{
    public CustomMessageBoxWindow(Styling styling)
    {
        var (background, enable, tint, opacity) = styling;
        //this shit is ridiculously scuffed
        Background = background;
        TransparencyLevelHint = enable ? [WindowTransparencyLevel.AcrylicBlur] : [];
        ExtendClientAreaToDecorationsHint = true;
        ExperimentalAcrylicBorder acrylic = new()
        {
            IsEnabled = enable,
            IsHitTestVisible = false,
            Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = (tint as ISolidColorBrush)!.Color,
                TintOpacity = 1,
                MaterialOpacity = opacity
            }
        };
        
        StackPanel navBar = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 10, 0, 0),
            IsVisible = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            Children =
            {
                new Image
                {
                    Source = new Bitmap(AssetLoader.Open(new Uri("avares://Henson/Assets/henson-icon.ico"))),
                    Width = 16,
                    Margin = new Thickness(0, 0, 10, 1)
                },
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("ContentTitle"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
        DockPanel.SetDock(navBar, Dock.Top);
        Dispatcher.UIThread.Post(() => (Content as DockPanel)?.Children.Insert(0, navBar));
        Dispatcher.UIThread.Post(() => (Content as DockPanel)?.Children.Insert(0, acrylic));
    }
}