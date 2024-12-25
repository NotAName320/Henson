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