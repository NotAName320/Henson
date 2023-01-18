using Henson.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Tmds.DBus;

namespace Henson.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<Nation> nations;

        public List<Nation> Nations
        {
            get { return nations; }
            set { nations = value; OnPropertyChanged(); }
        }

        private bool? isAllSelected;

        public bool? IsAllSelected
        {
            get { return isAllSelected; }
            set
            {
                isAllSelected = value;
                OnPropertyChanged();
            }
        }

        public ICommand CheckNationCommand { get; private set; }
        public ICommand CheckAllNationsCommand { get; private set; }

        public MainWindowViewModel()
        {
            Nations = new List<Nation>()
            {
                new Nation("a", "a", "a", "a", true),
                new Nation("b", "b", "b", "b", false),
                new Nation("c", "c", "c", "c", false),
                new Nation("d", "d", "d", "d", false),
            };

            //CheckNationCommand = new ICommand(OnCheckStudent);
            //CheckAllNationsCommand = new ICommand(OnCheckAllStudents);
            IsAllSelected = false;
        }

        private void OnCheckAllStudents()
        {
            if (IsAllSelected == true)
                Nations.ForEach(x => x.IsChecked = true);
            else
                Nations.ForEach(x => x.IsChecked = false);
        }

        private void OnCheckStudent()
        {
            if (Nations.All(x => x.IsChecked))
                IsAllSelected = true;
            else if (Nations.All(x => !x.IsChecked))
                IsAllSelected = false;
            else
                IsAllSelected = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void OnAddNationClick()
        {
            Console.WriteLine("Login");
        }

        public void OnLoginClick()
        {
            Console.WriteLine("Login");
        }

        public void OnFindWAClick()
        {
            Console.WriteLine("Login");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
