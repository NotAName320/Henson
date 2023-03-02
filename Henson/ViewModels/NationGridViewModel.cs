using Henson.Models;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class NationGridViewModel : ViewModelBase
    {
        private readonly Nation _nation;

        public NationGridViewModel(Nation nation, bool _checked, MainWindowViewModel parent)
        {
            _nation = nation;
            this._checked = _checked;
            Parent = parent; //This is a dumb solution to a dumb problem: not being able to access parent VMs in a DataGrid.

            Login = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationLoginClick(this);
            });

            ApplyWA = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationApplyWAClick(this);
            });

            GetLocalID = ReactiveCommand.CreateFromTask(async () =>
            {
                await Parent.OnNationGetLocalIDClick(this);
            });

            MoveTo = ReactiveCommand.CreateFromTask<string>(MoveToTask);
        }

        public string Name => _nation.Name;
        public string Pass => _nation.Pass;
        public string Region
        {
            get => _nation.Region;
            set
            {
                if(value == _nation.Region) return;
                _nation.Region = value;
                this.RaisePropertyChanged();
            }
        }
        public string? Chk { get; set; } = null;

        public ICommand Login { get; }
        public ICommand ApplyWA { get; }
        public ICommand GetLocalID { get; }
        public ReactiveCommand<string, Unit> MoveTo { get; }

        public async Task MoveToTask(string region)
        {
            await Parent.OnNationMoveRegionClick(this, region);
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }

        public MainWindowViewModel Parent { get; set; }
    }
}
