using Henson.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Henson.ViewModels
{
    public class NationGridViewModel : ViewModelBase
    {
        private readonly Nation _nation;

        public NationGridViewModel(Nation nation, bool _checked)
        {
            _nation = nation;
            this._checked = _checked;
        }

        public string Name => _nation.Name;
        public string FlagUrl => _nation.FlagUrl;
        public string Pass => _nation.Pass;
        public string Region => _nation.Region;

        public void OnCheckboxClick() //Saved for adding checkbox to header later
        {
        }

        public void Login()
        {
            System.Diagnostics.Debug.WriteLine("Login " + Name + " " + Pass);
        }

        public void ApplyWA()
        {
            System.Diagnostics.Debug.WriteLine("Apply WA " + Name + " " + Pass);
        }

        public void MoveTo(string region)
        {
            System.Diagnostics.Debug.WriteLine("MoveTo " + region);
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }

    }
}
