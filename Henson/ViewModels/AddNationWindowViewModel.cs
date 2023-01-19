using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace Henson.ViewModels
{
    public class AddNationWindowViewModel : ViewModelBase
    {
        public AddNationWindowViewModel()
        {
            ShowFilePickerDialog = new Interaction<FilePickerViewModel, string[]?>();

            FilePickerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new FilePickerViewModel();

                var result = await ShowFilePickerDialog.Handle(dialog);
            });

        }

        public ICommand FilePickerCommand { get; }

        public Interaction<FilePickerViewModel, string[]?> ShowFilePickerDialog { get; }
    }
}
