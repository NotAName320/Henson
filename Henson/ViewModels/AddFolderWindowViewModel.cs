using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Henson.ViewModels;

public class AddFolderWindowViewModel : ViewModelBase
{
    public string FolderNameBox { get; set; } = "";

    private readonly HashSet<string> Folders;
    
    public ReactiveCommand<Unit, string?> AddFolderCommand { get; }
    
    public ReactiveCommand<Unit, string?> CancelCommand { get; }
    
    public AddFolderWindowViewModel(IEnumerable<string>? folders, Styling styling)
    {
        (BackgroundColor, EnableAcrylic, AcrylicTint, AcrylicOpacity) = styling;
        AcrylicTransparency = EnableAcrylic ? [WindowTransparencyLevel.AcrylicBlur] : [];
        
        Folders = (folders ?? []).Select(x => x.ToLower()).ToHashSet();

        AddFolderCommand = ReactiveCommand.CreateFromTask<string?>(async () =>
        {
            var sanitizedFolderName = FolderNameBox.Trim();
            if(string.IsNullOrWhiteSpace(sanitizedFolderName))
            {
                MessageBoxViewModel messageDialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "No Folder Entered",
                    ContentMessage = "Please enter a folder name.",
                    Icon = Icon.Error,
                    Width = 350
                }, styling);
                await MessageBoxDialog.Handle(messageDialog);
                return null;
            }

            if(Folders.Contains(sanitizedFolderName))
            {
                MessageBoxViewModel messageDialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Folder Already Exists",
                    ContentMessage = "The folder name already exists.",
                    Icon = Icon.Error,
                    Width = 350
                }, styling);
                await MessageBoxDialog.Handle(messageDialog);
                return null;
            }
            
            return sanitizedFolderName;
        });
        
        CancelCommand = ReactiveCommand.Create<string?>(() => null);
    }
}