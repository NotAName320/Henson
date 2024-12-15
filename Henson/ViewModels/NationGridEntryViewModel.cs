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
    public readonly NationGridViewModel? RepresentedNation;

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
        NationGridViewModel? representedNation = null)
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