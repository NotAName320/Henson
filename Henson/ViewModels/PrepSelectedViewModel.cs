using Henson.Models;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class PrepSelectedViewModel : ViewModelBase
    {
        private NsClient Client { get; }
        private List<NationLoginViewModel> Nations { get; set; }
        private int LoginIndex { get; set; } = 0;
        private int PrepSuccesses { get; set; } = 0;
        private string CurrentChk { get; set; } = "";
        private string CurrentLocalID { get; set; } = "";
        private StringBuilder FailedLogins { get; set; } = new();
        public ICommand ActionButtonCommand { get; }
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();


        private string buttonText = "Login";
        public string ButtonText
        {
            get => buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonText, value);
            }
        }

        private bool alsoApplyWA = true;
        public bool AlsoApplyWA
        {
            get => alsoApplyWA;
            set
            {
                this.RaiseAndSetIfChanged(ref alsoApplyWA, value);
            }
        }

        private string currentLogin = "";
        public string CurrentLogin
        {
            get => currentLogin;
            set
            {
                this.RaiseAndSetIfChanged(ref currentLogin, value);
            }
        }

        private string targetRegion;
        public string TargetRegion
        {
            get => targetRegion;
            set
            {
                this.RaiseAndSetIfChanged(ref targetRegion, value);
            }
        }

        private string footerText = "Click Login to start.";
        public string FooterText
        {
            get => footerText;
            set
            {
                this.RaiseAndSetIfChanged(ref footerText, value);
            }
        }

        private bool buttonsEnabled = true;
        public bool ButtonsEnabled
        {
            get => buttonsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonsEnabled, value);
            }
        }

        public PrepSelectedViewModel(List<NationLoginViewModel> nations, NsClient client, string target)
        {
            Nations = nations;
            Client = client;
            targetRegion = target;

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (LoginIndex == Nations.Count)
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Logins Complete",
                        ContentMessage = $"All nations have been prepped. Please close the window now.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationLoginViewModel currentNation = Nations[LoginIndex];

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(buttonText)
                {
                    case "Login":
                        var (chk, localId) = Client.Login(currentNation) ?? default;
                        if(chk != null)
                        {
                            CurrentChk = chk;
                            CurrentLocalID = localId;
                            CurrentLogin = currentNation.Name;

                            FooterText = $"Logged in to {currentNation.Name}.";
                            ButtonText = AlsoApplyWA ? "Apply WA" : "Move to JP";
                        }
                        else
                        {
                            FooterText = $"Login to {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                        }
                        break;
                    case "Apply WA":
                        if(Client.ApplyWA(CurrentChk))
                        {
                            ButtonText = "Move to JP";
                            FooterText = $"Sent WA application on {currentNation.Name}.";
                        }
                        else
                        {
                            FooterText = $"{currentNation.Name} WA App failed.";
                            AddToFailedLogins(currentNation.Name);

                            ButtonText = "Login";
                            CurrentLogin = "";
                            LoginIndex++;
                        }
                        break;
                    case "Move to JP":
                        if(TargetRegion == "")
                        {
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Target Region Not Set",
                                ContentMessage = "Please set a target region.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            return;
                        }

                        if(!Client.MoveToJP(TargetRegion, CurrentLocalID))
                        {
                            FooterText = $"Moving {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                        }
                        else
                        {
                            FooterText = $"Moved {currentNation.Name} to {TargetRegion}.";
                            PrepSuccesses++;
                        }
                        ButtonText = "Login";
                        LoginIndex++;
                        if(LoginIndex == Nations.Count)
                        {
                            FooterText += $" {PrepSuccesses}/{Nations.Count} prepped successfully!";
                        }
                        break;
                }

                if (LoginIndex == Nations.Count && FailedLogins.Length != 0)
                {
                    FailedLogins.Remove(FailedLogins.Length - 2, 2);
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Some Logins Failed",
                        ContentMessage = $"The following nations failed to be prepped ({Nations.Count-PrepSuccesses}/{Nations.Count}):" +
                        $"\n{FailedLogins.ToString()}",
                        Icon = Icon.Warning,
                    });
                    await MessageBoxDialog.Handle(dialog);

                }

                ButtonsEnabled = true;
            });
        }

        private void AddToFailedLogins(string loginName)
        {
            if(FailedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                FailedLogins.Append("\n");
            }
            FailedLogins.Append(loginName + ", ");
        }
    }
}
