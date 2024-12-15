/*
View Model Representing Grid Row
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


using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Henson.ViewModels;

public class NationGridEntryViewModel : ViewModelBase
{
    private readonly string? _name;
    public readonly ObservableCollection<NationGridEntryViewModel> Items;
    public readonly NationViewModel? RepresentedNation;

    public string DisplayName => IsNation ? RepresentedNation!.GridName : "\ud83d\udcc1" + _name!;
    public bool IsNation => RepresentedNation != null;
    
    public bool IsSelected
    {
        get => IsNation ? RepresentedNation!.Checked : Items.All(x => x.IsSelected) && Items.Count != 0;
        set
        {
            if(IsNation)
            {
                RepresentedNation!.Checked = value;
            }
            this.RaisePropertyChanged();
        }
    }

    public NationGridEntryViewModel(string? name = null,
        ObservableCollection<NationGridEntryViewModel>? items = null,
        NationViewModel? representedNation = null)
    {
        _name = name;
        Items = items ?? [];
        RepresentedNation = representedNation;
        
        //change folder check box if something moves into folder
        Items.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(IsSelected));
        
        //change folder check box if child checkbox is checked
        Items.ToObservableChangeSet().AutoRefresh(x => x.IsSelected)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSelected)));
    }
    
    /// <summary>
    /// Add item to folder at startup. Precondition: this NationGridEntryViewModel is a folder.
    /// </summary>
    /// <param name="item">The item to add. Precondition: this item is a nation.</param>
    public void AddIntoFolder(NationGridEntryViewModel item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Triggers when checkmark button is clicked, used to implement select folder functionality
    /// </summary>
    public void OnCheckmarkClick()
    {
        //can't do this in setter. i tried.
        if(IsNation) return;

        var opposite = !IsSelected;
        foreach(var nation in Items)
        {
            nation.IsSelected = opposite;
        }
        this.RaisePropertyChanged(nameof(IsSelected));
    }
}