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