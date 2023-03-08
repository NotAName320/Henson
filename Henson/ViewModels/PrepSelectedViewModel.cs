using Henson.Models;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class PrepSelectedViewModel : ViewModelBase
    {
        private NsClient Client { get; }
        private List<NationLoginViewModel> Nations { get; set; }
        private int LoginIndex { get; set; } = 0;
        private string CurrentChk { get; set; } = "";
        private string CurrentLocalID { get; set; } = "";
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
                        ContentTitle = "Logins complete",
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

                            FooterText = $"Logged in to {currentNation.Name}";
                            ButtonText = AlsoApplyWA ? "Apply WA" : "Move to JP";
                        }
                        else
                        {
                            FooterText = LoginFooterText();
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Login Failed",
                                ContentMessage = $"Logging in to {currentNation.Name} failed. Going to the next nation.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            LoginIndex++;
                        }
                        break;
                    case "Apply WA":
                        if(Client.ApplyWA(CurrentChk))
                        {
                            ButtonText = "Move to JP";
                            FooterText = $"Sent WA application on {currentNation.Name}";
                        }
                        else
                        {
                            FooterText = LoginFooterText();
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Login Failed",
                                ContentMessage = $"Applying to WA on {currentNation.Name} failed. Going to the next nation.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            
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
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Moving Nation To Region Failed",
                                ContentMessage = $"Moving the nation {currentNation.Name} to region {TargetRegion} failed.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            return;
                        }

                        ButtonText = "Login";
                        LoginIndex++;
                        FooterText = LoginIndex != Nations.Count ? $"Moved {currentNation.Name} to {TargetRegion}!" : "All nations logged in!";

                        break;
                }
                ButtonsEnabled = true;
            });
        }

        public string LoginFooterText()
        {
            if(LoginIndex == Nations.Count)
            {
                return "All nations logged in!";
            }
            return $"Next nation: {Nations[LoginIndex].Name}.";
        }
    }
}
