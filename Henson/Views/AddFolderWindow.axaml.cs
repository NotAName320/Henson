/*
Add Folder Window control
Copyright (C) 2023-24 NotAName320

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


using Avalonia.Controls;
using Henson.ViewModels;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;

namespace Henson.Views;

public partial class AddFolderWindow : HensonWindow<AddFolderWindowViewModel>
{
    public AddFolderWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.AddFolderCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
    }

    protected override Task ShowMessageBoxDialog(IInteractionContext<MessageBoxViewModel, ButtonResult> interaction)
    {
        SetClosing(true);
        return base.ShowMessageBoxDialog(interaction);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        //Spent like 4 hours figuring out that I needed the below line lol
        SetClosing(false);
    }
    
    private void SetClosing(bool value)
    {
        Closing += (_, e) => { e.Cancel = value; };
    }
}