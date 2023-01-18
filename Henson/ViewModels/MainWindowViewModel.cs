using Henson.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public List<Nation> Nations => new()
        {
            new Nation("a", "a", "a", "a", true),
            new Nation("b", "b", "b", "b", false),
            new Nation("c", "c", "c", "c", false),
            new Nation("d", "d", "d", "d", false),
        };

        public void OnAddNationClick()
        {
            System.Diagnostics.Debug.WriteLine("OnAddNationClick");
        }

        public void OnLoginSelectedClick()
        {
            System.Diagnostics.Debug.WriteLine("OnLoginSelectedClick");
        }

        public void OnFindWAClick()
        {
            System.Diagnostics.Debug.WriteLine("OnFindWAClick");
        }
    }
}
