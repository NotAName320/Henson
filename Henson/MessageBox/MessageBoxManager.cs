using Avalonia.Controls;
using Avalonia.Media;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.ViewModels;

namespace Henson.MessageBox;

public static class MessageBoxManager
{
    public static IMsBox<ButtonResult> GetMessageBoxStandard(MessageBoxStandardParams @params, Styling styling)
    {
        @params.Width = 400;
        var msBoxStandardViewModel = new MsBoxStandardViewModel(@params);
        var msBoxStandardView = new MsBoxStandardView
        {
            DataContext = msBoxStandardViewModel
        };
        return new CustomMsBox<MsBoxStandardView, MsBoxStandardViewModel, ButtonResult>(msBoxStandardView,
            msBoxStandardViewModel, styling);
    }

    /// <summary>
    /// Create instance of standard messagebox window
    /// </summary>
    /// <param name="title"> Windows title </param>
    /// <param name="text"> Text of messagebox body </param>
    /// <param name="enum"> Buttons of messagebox (default OK) </param>
    /// <param name="icon"> Icon of messagebox (default no icon) </param>
    /// <param name="windowStartupLocation"> Startup location of messagebox (default center screen) </param>
    /// <param name="styling"></param>
    /// <returns></returns>
    /// <remarks>
    /// Recommended method for message box
    /// </remarks>
    public static IMsBox<ButtonResult> GetMessageBoxStandard(string title, string text, Styling styling,
        ButtonEnum @enum = ButtonEnum.Ok, Icon icon = Icon.None,
        WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen) =>
        GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = @enum,
            Icon = icon,
            WindowStartupLocation = windowStartupLocation
        }, styling);
}